using System.ComponentModel.DataAnnotations;

namespace Hearty_Bites.Models
{
    public class Caterer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Shop Name is required")]
        [StringLength(100)]
        public string ShopName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public string UserId { get; set; }= string.Empty;
        public User User { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
    }
}