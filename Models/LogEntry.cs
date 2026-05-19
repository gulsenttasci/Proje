using System;

namespace Hearty_Bites.Models
{
    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Level { get; set; } = "INFO";
        public string Message { get; set; } = string.Empty;
        public string? UserId { get; set; }
    }
}