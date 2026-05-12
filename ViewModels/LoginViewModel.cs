using System.ComponentModel.DataAnnotations;

namespace Hearty_Bites.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email adress.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required..")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}