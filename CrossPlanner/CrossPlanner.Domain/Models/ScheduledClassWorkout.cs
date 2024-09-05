using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("ScheduledClassWorkout")]
    public class ScheduledClassWorkout
    {
        [Key]
        public int ScheduledClassWorkoutId { get; set; }

        public int WorkoutId { get; set; }

        public int ScheduledClassId { get; set; }

        [ForeignKey("WorkoutId")]
        public Workout Workout { get; set; }

        [ForeignKey("ScheduledClassId")]
        public ScheduledClass ScheduledClass { get; set; }
    }
}