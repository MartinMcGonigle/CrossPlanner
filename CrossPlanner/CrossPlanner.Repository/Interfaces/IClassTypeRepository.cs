using CrossPlanner.Domain.Models;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IClassTypeRepository : IRepositoryBase<ClassType>
    {
        public List<ClassType> GetAffiliateActiveClassTypes(int affiliateId);
    }
}