using System.ComponentModel.DataAnnotations;
// ✅ REMOVE: using System.Web.Mvc;

namespace BinIT2WinIT.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        // ✅ NEW: Expected Role for validation
        public string ExpectedRole { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        // ✅ USE FULLY QUALIFIED NAME
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Role")]
        public string RoleName { get; set; }

        [Display(Name = "Referral Code")]
        public string ReferralCode { get; set; }

        // Address Fields
        [Display(Name = "Street Address")]
        public string Address { get; set; }

        [Display(Name = "Suburb")]
        public string Suburb { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Province")]
        public string Province { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        // Community Selection
        [Display(Name = "Your Community")]
        public int? DropOffPointId { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        // ✅ USE FULLY QUALIFIED NAME
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}