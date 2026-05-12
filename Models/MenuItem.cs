using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace Hearty_Bites.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Food name is required")]
        public string FoodName { get; set; }= string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 10000, ErrorMessage = "Price must be positive")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        public int CatererId { get; set; }

        [ValidateNever]
        public Caterer? Caterer { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}