using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("Refund")]
    public class Refund
    {
        [Key]
        public int RefundId { get; set; }

        public int MembershipId { get; set; }

        public decimal Amount { get; set; }

        public DateTime RefundDate { get; set; }

        public string Reason { get; set; }

        [ForeignKey("MembershipId")]
        public Membership Membership { get; set; }
    }
}