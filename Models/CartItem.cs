
using System.ComponentModel.DataAnnotations.Schema;
namespace Hearty_Bites.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        
        
        public string UserId { get; set; } = string.Empty; 
        
        
        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }
        
        
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        
        public string CustomizationSummary { get; set; } = string.Empty;
    }
}