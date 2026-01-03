using System.ComponentModel.DataAnnotations;

namespace HarvestHub.WebApp.Models
{
    public class GovernmentScheme
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string TitleUrdu { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? DescriptionUrdu { get; set; }

        // Scheme details
        public string? Eligibility { get; set; }
        public string? Benefits { get; set; }
        public string? HowToApply { get; set; }
        public string? ContactInfo { get; set; }
        public string? OfficialLink { get; set; }

        // Category: Subsidy, Loan, Insurance, Training, etc.
        public string Category { get; set; } = "General";

        // Status: Active, Upcoming, Expired
        public string Status { get; set; } = "Active";

        // Dates
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // For sorting/display
        public bool IsFeatured { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
    }
}
