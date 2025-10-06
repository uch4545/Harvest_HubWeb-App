using System.ComponentModel.DataAnnotations;

namespace HarvestHub.WebApp.ViewModels
{
    public class BuyerRegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "CNIC is required.")]
        [Display(Name = "CNIC")]
        [RegularExpression(@"^\d{5}-\d{7}-\d{1}$", ErrorMessage = "CNIC must be in 35201-1234567-1 format.")]
        public string CNIC { get; set; }

        [Required(ErrorMessage = "PhoneNumber is required.")]
        [Display(Name = "PhoneNumber")]
        [RegularExpression(@"^\d{4}-\d{7}$", ErrorMessage = "PhoneNumber must be in 0321-1234567 format.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; }


        [Required(ErrorMessage = "Role Type is required.")]
        [Display(Name = "Role Type")]
        public string RoleType { get; set; } = "Buyer";
    }
}
