using CrossPlanner.Domain.Models;

namespace CrossPlanner.Domain.OtherModels
{
    public class DailyWorkoutViewModel
    {
        public DateTime SelectedDate { get; set; }

        public List<Workout> Workouts { get; set; } = new List<Workout>();
    }
}