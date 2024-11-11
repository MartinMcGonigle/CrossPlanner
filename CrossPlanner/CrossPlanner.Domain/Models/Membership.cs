using CrossPlanner.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("Membership")]
    public class Membership
    {
        [Key]
        public int MembershipId { get; set; }

        public bool IsActive { get; set; } = false;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool AutoRenew { get; set; } = false;

        public string MemberId { get; set; }

        public int MembershipPlanId { get; set; }

        public string? LastPaymentId { get; set; }

        public decimal? LastPaymentAmount { get; set; }

        public PaymentStatus? PaymentStatus { get; set; }

        [ForeignKey("MemberId")]
        public ApplicationUser Member { get; set; }

        [ForeignKey("MembershipPlanId")]
        public MembershipPlan MembershipPlan { get; set; }

        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}