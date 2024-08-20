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
    }
}