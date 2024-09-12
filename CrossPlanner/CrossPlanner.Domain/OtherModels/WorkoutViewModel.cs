using CrossPlanner.Domain.Models;

namespace CrossPlanner.Domain.OtherModels
{
    public class WorkoutViewModel 
    {
        public List<ClassType>? ClassTypes { get; set; }

        public Workout Workout { get; set; }
    }
}