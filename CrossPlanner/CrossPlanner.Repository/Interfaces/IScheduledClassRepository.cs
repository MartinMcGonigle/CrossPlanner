using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IScheduledClassRepository : IRepositoryBase<ScheduledClass>
    {
        public List<ScheduledClass> GetAffiliateScheduledClassByDate(int affiliateId, DateTime dateTime);

        public IEnumerable<ScheduledClassDetail> GetAffiliateScheduledClassByDateMember(int affiliateId, DateTime dateTime, string memberId);

        public IEnumerable<ScheduledClassDetail> GetAffiliateScheduledClassByDateStaff(int affiliateId, DateTime dateTime);

        public ScheduledClass? GetScheduledClassByIdSP(int scheduledClassId);

        public ScheduledClass? GetScheduledClassById(int scheduledClassId);
    }
}