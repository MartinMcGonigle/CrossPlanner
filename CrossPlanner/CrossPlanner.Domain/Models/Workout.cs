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

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}