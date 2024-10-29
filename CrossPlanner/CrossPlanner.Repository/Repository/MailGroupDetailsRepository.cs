using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrossPlanner.Repository.Repository
{
    public class MailGroupDetailsRepository : RepositoryBase<MailGroupDetails>, IMailGroupDetailsRepository
    {
        public MailGroupDetailsRepository(ApplicationContext applicationContext) : base(applicationContext)
        {
            
        }

        public IEnumerable<MailGroupDetails> GetMailGroupDetails(string group)
        {
            var data = _applicationContext.MailGroupDetails.Where(m => m.MailGroup.MailGroupName == group)
                .Include(m => m.MailGroup)
                .Include(m => m.MailDetails);

            return data;
        }
    }
}