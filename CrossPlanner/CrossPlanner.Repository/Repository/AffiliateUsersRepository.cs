using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrossPlanner.Repository.Repository
{
    public class AffiliateUsersRepository : RepositoryBase<AffiliateUser>, IAffiliateUsersRepository
    {
        public AffiliateUsersRepository(ApplicationContext applicationContext) : base(applicationContext)
        {
            
        }

        public List<AffiliateUser> GetAffiliateActiveUsers(int affiliateId)
        {
            var data = _applicationContext.AffiliateUsers
                .Where(au => au.AffiliateId == affiliateId
                && au.IsActive)
                .Include(au => au.ApplicationUser);

            return data.ToList();
        }
    }
}