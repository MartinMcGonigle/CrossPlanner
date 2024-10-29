using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;

namespace CrossPlanner.Repository.Repository
{
    public class MailDetailsRepository : RepositoryBase<MailDetails>, IMailDetailsRepository
    {
        public MailDetailsRepository(ApplicationContext applicationContext) : base (applicationContext)
        {
            
        }
    }
}