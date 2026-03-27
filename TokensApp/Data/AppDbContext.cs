using Microsoft.EntityFrameworkCore;
using TokensApp.Models;

namespace TokensApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Register all your tables here as DbSet
    public DbSet<VendorCategory> VendorCategories { get; set; }
    public DbSet<Vendor>         Vendors           { get; set; }
    public DbSet<Appointment>    Appointments      { get; set; }
    public DbSet<VendorService> VendorServices     { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Map ENUM columns as strings in PostgreSQL
        modelBuilder.Entity<Appointment>()
            .Property(a => a.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Appointment>()
            .Property(a => a.BookingType)
            .HasConversion<string>();
    }
}
