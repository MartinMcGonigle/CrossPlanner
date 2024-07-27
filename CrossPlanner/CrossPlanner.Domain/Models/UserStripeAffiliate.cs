using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("UserStripeAffiliate")]
    public class UserStripeAffiliate
    {
        [Key]
        public int UserStripeAffiliateId {  get; set; }

        public string StripeCustomerId { get; set; }

        public string ApplicationUserId { get; set; }

        public int AffiliateId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser Member { get; set; }

        [ForeignKey("AffiliateId")]
        public Affiliate? Affiliate { get; set; }
    }
}