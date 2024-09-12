using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("Workout")]
    public class Workout
    {
        [Key]
        public int WorkoutId { get; set; }

        [Required]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public int ClassTypeId { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("ClassTypeId")]
        public ClassType? ClassType { get; set; }
    }
}