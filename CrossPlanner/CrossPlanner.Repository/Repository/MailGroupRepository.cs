using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;

namespace CrossPlanner.Repository.Repository
{
    public class MailGroupRepository : RepositoryBase<MailGroup>, IMailGroupRepository
    {
        public MailGroupRepository(ApplicationContext applicationContext) : base (applicationContext)
        {
            
        }
    }
}