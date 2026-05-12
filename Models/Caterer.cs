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

        [ValidateNever]
        public User? User { get; set; }
        public bool IsDeleted { get; set; } = false;

        [ValidateNever]
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}