using System.ComponentModel.DataAnnotations;

namespace Hearty_Bites.Models
{
    public class Comments
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please provide a rating.")]
        [Range(1, 5, ErrorMessage = "Please rate between 1 and 5 stars")]
        public int StarRating { get; set; }

        [StringLength(500, ErrorMessage = "Comment is too long")]
        public string? CommentText { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public int  CatererId { get; set; }
        public Caterer Caterer { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
    }
}