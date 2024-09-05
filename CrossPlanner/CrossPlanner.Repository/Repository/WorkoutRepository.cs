using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;

namespace CrossPlanner.Repository.Repository
{
    public class WorkoutRepository : RepositoryBase<Workout>, IWorkoutRepository
    {
        public WorkoutRepository(ApplicationContext applicationContext) : base(applicationContext)
        {
            
        }
    }
}