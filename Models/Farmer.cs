using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HarvestHub.WebApp.Models;

public class Farmer
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [Required]
    [StringLength(15, MinimumLength = 13)]
    public string CNIC { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string? ProfileImagePath { get; set; }

    public string? PhoneNumber { get; set; }

    public string? PasswordHash { get; set; }

    [Required]
    public string ApplicationUserId { get; set; }

    [ForeignKey("ApplicationUserId")]
    public ApplicationUser ApplicationUser { get; set; }
}

public enum VerificationStatus { Pending, Approved, Rejected }

public class VerificationDocument
{
    public int Id { get; set; }

    public string UserId { get; set; }   // FK
    public ApplicationUser User { get; set; }  // 🔹 Navigation property

    public string DocumentType { get; set; }
    public string FilePath { get; set; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string ReviewedBy { get; set; }
    public string Remarks { get; set; }
}


public class Laboratory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string LabName { get; set; }
    [Required]
    [MaxLength(200)]
    public string LicenseNumber { get; set; }
    public string ContactPerson { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public bool IsVerified { get; set; } = false;
    public string? WebsiteUrl { get; set; }   // Website / Social media link
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum CropVariety
{
    Wheat,
    Rice,
    Maize,
    Cotton,
    Sugarcane,
    Barley,
    Pulses,
    Vegetables,
    Fruits,
    Other
}
public class Crop
{
    public int Id { get; set; }
    public string Name { get; set; }

    public CropVariety Variety { get; set; }   // ✅ Enum use ho raha hai

    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    public decimal PricePerUnit { get; set; }

    public string Description { get; set; }    // ✅ New field

    // Farmer Relation
    public int FarmerId { get; set; }
    public Farmer Farmer { get; set; }

    // Lab Report Relation
    public int? ReportId { get; set; }
    public LabReport Report { get; set; }

    // Images
    public ICollection<CropImage> Images { get; set; }
}

public class CropImage
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }

    public int CropId { get; set; }
    public Crop Crop { get; set; }
}

public class LabReport
{
    public int Id { get; set; }

    public int FarmerId { get; set; }
    public Farmer Farmer { get; set; }

    public int LaboratoryId { get; set; }
    public Laboratory Laboratory { get; set; }

    public string ReportFilePath { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class LabReportAttachment
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] Data { get; set; }
    public DateTime UploadedOn { get; set; }

    // Relation to Lab
    public int LabId { get; set; }
    public Laboratory Lab { get; set; }
}
public class Order
{
    [Key]
    public int Id { get; set; }

    // Buyer relation
    [Required]
    public int BuyerId { get; set; }
    [ForeignKey("BuyerId")]
    public Buyer Buyer { get; set; }

    // Crop relation
    [Required]
    public int CropId { get; set; }
    // Order details
    public Crop Crop { get; set; }
    [Required]
    public decimal Quantity { get; set; }

    [Required]
    public decimal TotalPrice { get; set; }

    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string Status { get; set; } = "Pending";
}


public class ErrorLog
{
    public int Id { get; set; }
    public string? ControllerName { get; set; }
    public string? ActionName { get; set; }
    public string? UserId { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? StackTrace { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}