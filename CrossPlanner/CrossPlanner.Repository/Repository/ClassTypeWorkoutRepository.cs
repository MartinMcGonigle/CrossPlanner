using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;

namespace CrossPlanner.Repository.Repository
{
    public class ClassTypeWorkoutRepository : RepositoryBase<ClassTypeWorkout>, IClassTypeWorkoutRepository
    {
        public ClassTypeWorkoutRepository(ApplicationContext applicationContext) : base(applicationContext)
        {
        }
    }
}