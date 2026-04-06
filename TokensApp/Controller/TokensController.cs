using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TokensApp.Data;
using TokensApp.Models;

namespace TokensApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokensController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<TokensController> _logger;

    public TokensController(AppDbContext db, ILogger<TokensController> logger) 
    { 
        _db = db;
        _logger = logger;
    }

    // ─── API 1: GET All Vendor Categories ───────────────────────────────────
    /// <summary>Get all active vendor categories (Saloon, Doctor, Gym...)</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories() 
    {
        try
        {
            _logger.LogInformation("Attempting to fetch all active vendor categories.");
            var cats = await _db.VendorCategories
                .Where(c => c.IsActive)
                .Select(c => new { c.VendorCategoryId, c.CategoryName, c.CategoryDesc })
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            
            _logger.LogInformation("Successfully retrieved {Count} vendor categories.", cats.Count);
            return Ok(cats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching vendor categories.");
            return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    // ─── API 2: GET Vendors (with optional category_id & city filter) ────────
    /// <summary>Get vendors — filter by category_id or city</summary>
    [HttpGet("vendors")]
    public async Task<IActionResult> GetVendors(
        [FromQuery] int? categoryId,
        [FromQuery] string? city)
    {
        try
        {
            _logger.LogInformation("Attempting to fetch vendors with CategoryId: {CategoryId}, City: {City}", categoryId, city);
            var query = _db.Vendors
                .Include(v => v.Category)
                .Where(v => v.IsActive)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(v => v.VendorCategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(city))
                query = query.Where(v => v.City.ToLower().Contains(city.ToLower()));

            var result = await query.Select(v => new {
                v.VendorId, v.VendorName, v.OwnerName,
                v.City,     v.MobileNumber,
                Category = v.Category!.CategoryName
            }).ToListAsync();

            _logger.LogInformation("Successfully retrieved {Count} vendors.", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching vendors.");
            return StatusCode(500, new { message = "An internal error occurred while fetching vendors." });
        }
    }

    // ─── API 3: GET Vendor Services ──────────────────────────────────────────
    /// <summary>Get all active services for a specific vendor</summary>
    [HttpGet("vendors/{vendorId}/services")]
    public async Task<IActionResult> GetVendorServices(int vendorId)
    {
        try
        {
            _logger.LogInformation("Attempting to fetch services for VendorId: {VendorId}", vendorId);
            var vendor = await _db.Vendors.FindAsync(vendorId);
            if (vendor == null || !vendor.IsActive)
            {
                _logger.LogWarning("Vendor {VendorId} not found or inactive.", vendorId);
                return NotFound(new { message = $"Vendor {vendorId} not found or inactive." });
            }

            var services = await _db.VendorServices
                .Where(s => s.VendorId == vendorId && s.IsActive)
                .Select(s => new {
                    s.ServiceId, s.ServiceName,
                    s.ServiceDescription,
                    Price = s.Price,
                    s.DurationMins
                }).ToListAsync();

            _logger.LogInformation("Successfully retrieved {Count} services for VendorId: {VendorId}.", services.Count, vendorId);
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching services for VendorId: {VendorId}.", vendorId);
            return StatusCode(500, new { message = "An internal error occurred while fetching services." });
        }
    }

    // ─── API 4: POST Book Appointment (Create Token) ─────────────────────────
    /// <summary>Book an appointment and get a token number</summary>
    [HttpPost("appointments")]
    public async Task<IActionResult> BookAppointment([FromBody] BookingRequest req)
    {
        try
        {
            _logger.LogInformation("Attempting to book appointment for UserId: {UserId}, VendorId: {VendorId}, ServiceId: {ServiceId}.", req.UserId, req.VendorId, req.ServiceId);

            // Validations
            var vendor = await _db.Vendors.FindAsync(req.VendorId);
            if (vendor == null || !vendor.IsActive)
            {
                _logger.LogWarning("Vendor {VendorId} not found or inactive during booking.", req.VendorId);
                return BadRequest(new { message = "Invalid or inactive Vendor." });
            }

            var service = await _db.VendorServices.FindAsync(req.ServiceId);
            if (service == null || !service.IsActive || service.VendorId != req.VendorId)
            {
                _logger.LogWarning("Service {ServiceId} not found, inactive, or does not belong to Vendor {VendorId}.", req.ServiceId, req.VendorId);
                return BadRequest(new { message = "Invalid Service for this Vendor." });
            }

            // Get next token number for this vendor on this date
            var lastToken = await _db.Appointments
                .Where(a => a.VendorId == req.VendorId
                         && a.AppointmentDate == req.AppointmentDate)
                .MaxAsync(a => (short?)a.TokenNumber) ?? 0;

            var appointment = new Appointment {
                TokenNumber     = (short)(lastToken + 1),
                UserId          = req.UserId,
                VendorId        = req.VendorId,
                ServiceId       = req.ServiceId,
                AppointmentDate = req.AppointmentDate,
                Status          = "CONFIRMED",
                BookingType     = req.BookingType ?? "ONLINE",
                Notes           = req.Notes,
                CreatedBy       = req.CreatedBy ?? "App",
                CreatedDatetime = DateTime.UtcNow
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Successfully booked appointment {AppointmentId} with Token Number {TokenNumber}.", appointment.AppointmentId, appointment.TokenNumber);

            return CreatedAtAction(nameof(GetAppointmentById),
                new { id = appointment.AppointmentId },
                new {
                    appointment.AppointmentId,
                    appointment.TokenNumber,
                    appointment.Status
                });
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error occurred while booking appointment. Possible concurrency issue.");
            return StatusCode(500, new { message = "An error occurred while saving the appointment. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while booking appointment for VendorId: {VendorId}.", req.VendorId);
            return StatusCode(500, new { message = "An internal error occurred while booking the appointment." });
        }
    }

    // ─── API 5: GET Appointment by ID ────────────────────────────────────────
    [HttpGet("appointments/{id}")]
    public async Task<IActionResult> GetAppointmentById(int id)
    {
        try
        {
            _logger.LogInformation("Attempting to fetch appointment ID: {Id}", id);
            var appt = await _db.Appointments.FindAsync(id);
            if (appt == null) 
            {
                _logger.LogWarning("Appointment {Id} not found.", id);
                return NotFound();
            }
            return Ok(appt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching appointment ID: {Id}.", id);
            return StatusCode(500, new { message = "An internal error occurred." });
        }
    }

    // ─── API 6: PATCH Cancel Appointment ─────────────────────────────────────
    /// <summary>Cancel an appointment by ID</summary>
    [HttpPatch("appointments/{id}/cancel")]
    public async Task<IActionResult> CancelAppointment(
        int id, [FromBody] CancelRequest req)
    {
        try
        {
            _logger.LogInformation("Attempting to cancel appointment ID: {Id}", id);
            var appt = await _db.Appointments.FindAsync(id);
            if (appt == null)
            {
                _logger.LogWarning("Appointment {Id} not found for cancellation.", id);
                return NotFound(new { message = $"Appointment {id} not found" });
            }

            if (appt.Status == "COMPLETED" || appt.Status == "CANCELLED")
            {
                _logger.LogWarning("Attempted to cancel appointment {Id} which is already in {Status} status.", id, appt.Status);
                return BadRequest(new { message = $"Cannot cancel — status is {appt.Status}" });
            }

            appt.Status           = "CANCELLED";
            appt.ModifiedDatetime = DateTime.UtcNow;
            appt.ModifiedBy       = req.ModifiedBy ?? "App";

            await _db.SaveChangesAsync();

            _logger.LogInformation("Successfully cancelled appointment {Id}.", id);
            return Ok(new { appt.AppointmentId, appt.Status, Message = "Appointment cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling appointment ID: {Id}.", id);
            return StatusCode(500, new { message = "An internal error occurred while cancelling the appointment." });
        }
    }
}

// ─── Request/Response DTOs ──────────────────────────────────────────────────
public record BookingRequest(
    int      UserId,
    int      VendorId,
    int      ServiceId,
    DateOnly AppointmentDate,
    string?  BookingType,
    string?  Notes,
    string?  CreatedBy
);

public record CancelRequest(string? ModifiedBy);
