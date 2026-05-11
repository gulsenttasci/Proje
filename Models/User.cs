using System.ComponentModel.DataAnnotations; 

namespace Hearty_Bites.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")] 
        [StringLength(50, MinimumLength = 3)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")] 
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        // ^(?=.*[a-zA-Z])  en az bir harf içermeli
        // (?=.*\d)         en az bir rakam içermeli
        // [a-zA-Z0-9]+$    sadece harf ve rakam (özel karakterlere izin verilmez)
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)[a-zA-Z0-9]+$", 
        ErrorMessage = "Password must contain at least one letter and one number, and no special characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } 
    }
}