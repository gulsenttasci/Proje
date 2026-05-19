using System;

namespace Hearty_Bites.Models
{
    public class OrderRating
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int Score { get; set; } 
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}