using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("AffiliateUser")]
    public class AffiliateUser
    {
        [Key]
        public int AffiliateUserId { get; set; }

        public int AffiliateId { get; set; }

        public string ApplicationUserId { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey("AffiliateId")]
        public Affiliate Affiliate { get; set; }

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}