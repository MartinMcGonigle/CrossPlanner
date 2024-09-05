using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("ClassTypeWorkout")]
    public class ClassTypeWorkout
    {
        [Key]
        public int ClassTypeWorkoutId { get; set; }

        public int WorkoutId { get; set; }
        
        public int ClassTypeId { get; set; }

        public DateTime Date {  get; set; }

        [ForeignKey("WorkoutId")]
        public Workout Workout { get; set; }

        [ForeignKey("ClassTypeId")]
        public ClassType ClassType { get; set; }
    }
}