using CrossPlanner.Domain.Models;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IScheduledClassRepository : IRepositoryBase<ScheduledClass>
    {
        public List<ScheduledClass> GetAffiliateScheduledClassByDate(int affiliateId, DateTime dateTime);
    }
}