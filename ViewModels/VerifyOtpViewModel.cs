using System.ComponentModel.DataAnnotations;

namespace HarvestHub.WebApp.ViewModels
{
    public class VerifyOtpViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "OTP is required.")]
        [Display(Name = "OTP Code")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be a 6-digit numeric code.")]
        public string OtpCode { get; set; }
    }
}
