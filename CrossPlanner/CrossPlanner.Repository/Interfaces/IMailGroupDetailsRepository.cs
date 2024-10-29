using CrossPlanner.Domain.Models;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IMailGroupDetailsRepository : IRepositoryBase<MailGroupDetails>
    {
        public IEnumerable<MailGroupDetails> GetMailGroupDetails(string group);
    }
}