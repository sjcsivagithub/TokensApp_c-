using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TokensApp.Models;

[Table("vendor_categories")]
public class VendorCategory
{
    [Key]
    [Column("vendor_category_id")]
    public int VendorCategoryId { get; set; }

    [Column("category_name")]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Column("category_desc")]
    public string? CategoryDesc { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_datetime")]
    public DateTime CreatedDatetime { get; set; } = DateTime.UtcNow;

    [Column("modified_datetime")]
    public DateTime? ModifiedDatetime { get; set; }

    [Column("created_by")]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = "Admin";

    [Column("modified_by")]
    [MaxLength(100)]
    public string? ModifiedBy { get; set; }

    // Navigation property
    public ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();
}
