using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;

namespace CrossPlanner.Repository.Repository
{
    public class AffiliateRepository : RepositoryBase<Affiliate>, IAffiliateRepository
    {
        public AffiliateRepository(ApplicationContext applicationContext) : base(applicationContext)
        {
        }
    }
}