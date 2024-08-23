using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IApplicationUserRepository : IRepositoryBase<ApplicationUser>
    {
        public IEnumerable<ApplicationUser> GetAffiliateStaff(int affiliateId);

        public Task<IEnumerable<ApplicationUser>> GetDeactivatedUsers();

        public IEnumerable<AffiliateUsersViewModel> GetAffiliateUsers(string q, int affiliateId, int page, int pageSize, string linkedToGymSearch, string emailConfirmedSearch, string activeMembershipSearch, string roleSearch);

        public int GetAffiliateUsersCount(string q, int affiliateId, string linkedToGymSearch, string emailConfirmedSearch, string activeMembershipSearch, string roleSearch);

        public IEnumerable<ApplicationUser> GetAffiliateActiveStaff(int affiliateId);
    }
}