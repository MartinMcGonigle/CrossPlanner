using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrossPlanner.Repository.Repository
{
    public class ScheduledClassRepository : RepositoryBase<ScheduledClass>, IScheduledClassRepository
    {
        public ScheduledClassRepository(ApplicationContext applicationContext) : base (applicationContext)
        {
            
        }

        public List<ScheduledClass> GetAffiliateScheduledClassByDate(int affiliateId, DateTime dateTime)
        {
            var scheduledClasses = _applicationContext.ScheduledClasses
                .Where(sc =>
                sc.ClassType.AffiliateId == affiliateId &&
                sc.StartDateTime.Date == dateTime.Date &&
                !sc.IsDeleted)
                .Include(sc => sc.ClassType)
                .Include (sc => sc.Instructor)
                .OrderBy(sc => sc.StartDateTime)
                .AsNoTracking()
                .ToList();

            return scheduledClasses;
        }
    }
}