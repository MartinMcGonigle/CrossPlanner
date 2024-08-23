using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;

namespace CrossPlanner.Repository.Repository
{
    public class ClassTypeRepository : RepositoryBase<ClassType>, IClassTypeRepository
    {
        public ClassTypeRepository(ApplicationContext applicationContext) : base(applicationContext)
        {
            
        }

        public List<ClassType> GetAffiliateActiveClassTypes(int affiliateId)
        {
            var data = _applicationContext.ClassTypes.Where(ct => ct.IsActive
            && !ct.IsDeleted
            && ct.AffiliateId == affiliateId);

            return data.ToList();
        }
    }
}