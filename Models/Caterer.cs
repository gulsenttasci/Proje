using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Hearty_Bites.Models
{
    public class Caterer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Shop Name is required")]
        [StringLength(100)]
        public string ShopName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        [ValidateNever]
        public string UserId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [ValidateNever]
        public User? User { get; set; }
        public bool IsDeleted { get; set; } = false;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string? ImageUrl { get; set; }

        
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [ValidateNever]
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}