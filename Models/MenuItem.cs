using System.ComponentModel.DataAnnotations;

namespace Hearty_Bites.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required]
        public string FoodName { get; set; }

        [Range(0.01, 10000, ErrorMessage = "Price must be positive")]
        public decimal Price { get; set; }


        public int CatererId { get; set; }
        public Caterer Caterer { get; set; }
    }
}