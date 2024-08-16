using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IAffiliateRepository : IRepositoryBase<Affiliate>
    {
        public IEnumerable<AffiliateMembersViewModel> GetAffiliateMembers(string q, int affiliateId, int page, int pageSize);

        public int GetAffiliateMembersCount(string q, int affiliateId);
    }
}