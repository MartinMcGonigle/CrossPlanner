using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;

namespace CrossPlanner.Repository.Repository
{
    public class UserStripeAffiliateRepository : RepositoryBase<UserStripeAffiliate>, IUserStripeAffiliateRepository
    {
        public UserStripeAffiliateRepository(ApplicationContext applicationContext)
            : base(applicationContext)
        {
            
        }
    }
}