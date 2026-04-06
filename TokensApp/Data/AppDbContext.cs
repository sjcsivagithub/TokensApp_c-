using Microsoft.EntityFrameworkCore;
using TokensApp.Models;

namespace TokensApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<VendorCategory> VendorCategories { get; set; }
    public DbSet<Vendor>         Vendors           { get; set; }
    public DbSet<Appointment>    Appointments      { get; set; }
    public DbSet<VendorService>  VendorServices    { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── VendorCategory → Vendor (1 : many) ────────────────────────────────
        modelBuilder.Entity<Vendor>()
            .HasOne(v => v.Category)
            .WithMany(c => c.Vendors)
            .HasForeignKey(v => v.VendorCategoryId)
            .HasConstraintName("fk_vendor_category");

        // ── Vendor → VendorService (1 : many) ─────────────────────────────────
        modelBuilder.Entity<VendorService>()
            .HasOne(s => s.Vendor)
            .WithMany(v => v.Services)
            .HasForeignKey(s => s.VendorId)
            .HasConstraintName("fk_service_vendor");

        // ── Vendor → Appointment (1 : many) ───────────────────────────────────
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Vendor)
            .WithMany(v => v.Appointments)
            .HasForeignKey(a => a.VendorId)
            .HasConstraintName("fk_appointment_vendor");

        // ── Map status/booking_type as plain strings (not PostgreSQL enums) ───
        modelBuilder.Entity<Appointment>()
            .Property(a => a.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Appointment>()
            .Property(a => a.BookingType)
            .HasConversion<string>();
    }
}
