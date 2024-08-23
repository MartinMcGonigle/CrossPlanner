using CrossPlanner.Domain.Models;

namespace CrossPlanner.Domain.OtherModels
{
    public class ScheduledClassViewModel
    {
        public List<ApplicationUser>? Instructors { get; set; }

        public List<ClassType>? ClassTypes { get; set; }

        public ScheduledClass ScheduledClass { get; set; }
    }
}