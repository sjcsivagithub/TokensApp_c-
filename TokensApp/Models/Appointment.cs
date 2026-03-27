using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TokensApp.Models;

[Table("appointments")]
public class Appointment
{
    [Key][Column("appointment_id")]
    public int AppointmentId { get; set; }

    [Column("token_number")]
    public short TokenNumber { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("vendor_id")]
    public int VendorId { get; set; }

    [Column("service_id")]
    public int ServiceId { get; set; }

    [Column("appointment_date")]
    public DateOnly AppointmentDate { get; set; }

    [Column("status")]
    public string Status { get; set; } = "PENDING";

    [Column("booking_type")]
    public string BookingType { get; set; } = "ONLINE";

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_datetime")]
    public DateTime CreatedDatetime { get; set; } = DateTime.UtcNow;

    [Column("modified_datetime")]
    public DateTime? ModifiedDatetime { get; set; }

    [Column("created_by")][MaxLength(100)]
    public string CreatedBy { get; set; } = "Admin";

    [Column("modified_by")][MaxLength(100)]
    public string? ModifiedBy { get; set; }
}
