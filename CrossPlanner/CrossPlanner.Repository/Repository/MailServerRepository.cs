using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;

namespace CrossPlanner.Repository.Repository
{
    public class MailServerRepository : RepositoryBase<MailServer>, IMailServerRepository
    {
        public MailServerRepository(ApplicationContext applicationContext) : base(applicationContext)
        {
            
        }
    }
}