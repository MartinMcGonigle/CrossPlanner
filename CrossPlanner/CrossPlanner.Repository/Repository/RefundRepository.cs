using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;

namespace CrossPlanner.Repository.Repository
{
    public class RefundRepository : RepositoryBase<Refund>, IRefundRepository
    {
        public RefundRepository(ApplicationContext applicationContext) : base (applicationContext)
        {
            
        }
    }
}