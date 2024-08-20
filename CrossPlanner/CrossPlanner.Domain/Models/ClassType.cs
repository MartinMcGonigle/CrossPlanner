using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("ClassType")]
    public class ClassType
    {
        [Key]
        public int ClassTypeId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public int AffiliateId { get; set; }

        [ForeignKey("AffiliateId")]
        public Affiliate? Affiliate { get; set; }
    }
}