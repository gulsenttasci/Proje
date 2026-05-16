using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hearty_Bites.Models 
{
    public class CustomizationGroup
    {
        [Key]
        public int Id { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        [Required]
        [Display(Name = "Group Name")]
        public string GroupName { get; set; } = string.Empty; 

        
        public bool IsMultipleChoice { get; set; }
        public bool IsDeleted { get; set; } = false;
        public List<CustomizationOption> Options { get; set; } = new List<CustomizationOption>();
    }
}