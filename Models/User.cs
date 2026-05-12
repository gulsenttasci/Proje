using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Hearty_Bites.Models
{
    public class User: IdentityUser
    {
    

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(50, MinimumLength = 3)]
        public string FullName { get; set; } = string.Empty;
   
        [Required]
        public string Role { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
    }
}