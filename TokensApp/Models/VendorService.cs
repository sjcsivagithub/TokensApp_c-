using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TokensApp.Models;

[Table("vendor_services")]
public class VendorService
{
    [Key][Column("service_id")]
    public int ServiceId { get; set; }

    [Column("vendor_id")]
    public int VendorId { get; set; }

    [Column("service_name")][MaxLength(150)]
    public string ServiceName { get; set; } = string.Empty;

    [Column("service_description")]
    public string? ServiceDescription { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("duration_mins")]
    public short DurationMins { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_datetime")]
    public DateTime CreatedDatetime { get; set; } = DateTime.UtcNow;

    [Column("modified_datetime")]
    public DateTime? ModifiedDatetime { get; set; }

    [Column("created_by")][MaxLength(100)]
    public string CreatedBy { get; set; } = "Admin";

    [Column("modified_by")][MaxLength(100)]
    public string? ModifiedBy { get; set; }

    // Navigation property
    public Vendor? Vendor { get; set; }
}
