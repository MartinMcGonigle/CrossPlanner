using CrossPlanner.Domain.Models;

namespace CrossPlanner.Domain.OtherModels
{
    public class MembershipSelectionViewModel
    {
        public List<MembershipPlan> MembershipPlans { get; set; }

        public Membership Membership { get; set; }
    }
}