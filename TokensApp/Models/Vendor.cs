using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TokensApp.Models;

[Table("vendors")]
public class Vendor
{
    [Key][Column("vendor_id")]
    public int VendorId { get; set; }

    [Column("vendor_category_id")]
    public int VendorCategoryId { get; set; }

    [Column("vendor_name")][MaxLength(150)]
    public string VendorName { get; set; } = string.Empty;

    [Column("owner_name")][MaxLength(150)]
    public string OwnerName { get; set; } = string.Empty;

    [Column("city")][MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Column("mobile_number")][MaxLength(15)]
    public string MobileNumber { get; set; } = string.Empty;

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

    // Navigation properties
    public VendorCategory? Category { get; set; }
    public ICollection<VendorService> Services { get; set; } = new List<VendorService>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
