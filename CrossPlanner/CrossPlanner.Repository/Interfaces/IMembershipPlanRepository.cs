using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IMembershipPlanRepository : IRepositoryBase<MembershipPlan>
    {
        public IEnumerable<MembershipPlan> GetAffiliateMembershipPlans(int affiliateId, string q, int page, int pageSize, string statusSearch, string typeSearch);

        public int GetAffiliateMembershipPlansCount(int affiliateId, string q, string statusSearch, string typeSearch);
    }
}