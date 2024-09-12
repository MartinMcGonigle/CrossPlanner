using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CrossPlanner.Domain.OtherModels
{
    public class WorkoutFormViewModel
    {
        [Required]
        public string Description { get; set; }

        public int? ClassTypeId { get; set; }

        public int? ScheduledClassId { get; set; }

        public DateTime? WorkoutDate { get; set; }

        public IEnumerable<SelectListItem>? ClassTypes { get; set; }

        public IEnumerable<SelectListItem>? ScheduledClasses { get; set; }

        public bool? IsClassTypeWorkout { get; set; }
    }
}