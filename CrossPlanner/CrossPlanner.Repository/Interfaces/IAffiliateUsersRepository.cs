using CrossPlanner.Domain.Models;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IAffiliateUsersRepository : IRepositoryBase<AffiliateUser>
    {
        public List<AffiliateUser> GetAffiliateActiveUsers(int affiliateId);
    }
}