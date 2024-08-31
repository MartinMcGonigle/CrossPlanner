using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IMembershipRepository : IRepositoryBase<Membership>
    {
        public IEnumerable<MembershipViewModel> GetAffiliateMemberships(string q, int affiliateId, int page, int pageSize);

        public int GetAffiliateMembershipsCount(string q, int affiliateId, int page, int pageSize);

        public IEnumerable<Membership> GetUserMemberships(int affiliateId, string memberId);

        public Membership? GetMembershipByAffiliateMemberId(int affiliateId, string memberId);
    }
}