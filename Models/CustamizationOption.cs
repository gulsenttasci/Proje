using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hearty_Bites.Models
{
    public class CustomizationOption
    {
        [Key]
        public int Id { get; set; }

        public int CustomizationGroupId { get; set; }
        public CustomizationGroup? CustomizationGroup { get; set; }

        [Required]
        public string OptionName { get; set; } = string.Empty;


        [Column(TypeName = "decimal(18,2)")]
        public decimal AdditionalPrice { get; set; } = 0; 
        
        public bool IsDeleted { get; set; } = false;
    }
}