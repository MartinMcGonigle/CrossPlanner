using CrossPlanner.Domain.Models;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IApplicationUserRepository : IRepositoryBase<ApplicationUser>
    {
        public IEnumerable<ApplicationUser> GetAffiliateStaff(int affiliateId);

        public Task<IEnumerable<ApplicationUser>> GetDeactivatedUsers();

        public IEnumerable<ApplicationUser> GetAffiliateMembers(int affiliateId);
    }
}