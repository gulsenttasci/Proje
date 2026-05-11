using System.ComponentModel.DataAnnotations;

namespace Hearty_Bites.Models
{
    public class Caterer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Shop Name is required")]
        [StringLength(100)]
        public string ShopName { get; set; }

        [Required]
        public string Description { get; set; } 

        public string Address { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}