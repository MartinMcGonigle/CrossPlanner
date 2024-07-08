using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;

namespace CrossPlanner.Repository.Repository
{
    public class AffiliateUsersRepository : RepositoryBase<AffiliateUser>, IAffiliateUsersRepository
    {
        public AffiliateUsersRepository(ApplicationContext applicationContext) : base(applicationContext)
        {
            
        }
    }
}