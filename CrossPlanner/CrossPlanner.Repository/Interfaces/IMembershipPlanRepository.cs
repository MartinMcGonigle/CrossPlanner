using CrossPlanner.Domain.Models;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IMembershipPlanRepository : IRepositoryBase<MembershipPlan>
    {
        public IEnumerable<MembershipPlan> GetAffiliateMembershipPlans(int affiliateId);
    }
}