using System.ComponentModel.DataAnnotations.Schema;

namespace Hearty_Bites.Models
{
    public class Order
    {
        public int Id { get; set; }


        public string UserId { get; set; } = string.Empty;


        public int CatererId { get; set; }
        public Caterer? Caterer { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime EventDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }


        public string Status { get; set; } = "Pending";


        public string DeliveryAddress { get; set; } = string.Empty;


        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}