using System.ComponentModel.DataAnnotations;

namespace NyayaSetu.ViewModels
{
    public class RegisterViewModel
    {
        [Required, StringLength(120)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone, Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(80)]
        public string? City { get; set; }

        [StringLength(80)]
        public string? State { get; set; }

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ProfileViewModel
    {
        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(80)]
        public string? City { get; set; }

        [StringLength(80)]
        public string? State { get; set; }

        [StringLength(12)]
        public string? PinCode { get; set; }

        public string PreferredLanguage { get; set; } = "en";
    }
}
