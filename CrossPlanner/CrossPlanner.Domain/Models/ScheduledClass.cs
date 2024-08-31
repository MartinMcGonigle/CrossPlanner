using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("ScheduledClass")]
    public class ScheduledClass
    {
        [Key]
        public int ScheduledClassId { get; set; }

        [Required]
        public int ClassTypeId { get; set; }
        
        [Required]
        public string InstructorId { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int? Capacity { get; set; } // Allow null values for unlimited capacity

        public bool IsActive { get; set; } = true;

        public bool IsCancelled  { get; set; } = false;

        public string? CancellationReason { get; set; }

        public bool IsDeleted { get; set; } = false;

        [ForeignKey("ClassTypeId")]
        public ClassType? ClassType { get; set; }
        
        [ForeignKey("InstructorId")]
        public ApplicationUser? Instructor {  get; set; }

        [NotMapped]
        public int ReservationsCount { get; set; }
    }
}