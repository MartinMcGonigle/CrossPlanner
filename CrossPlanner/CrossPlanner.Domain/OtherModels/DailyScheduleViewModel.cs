using CrossPlanner.Domain.Models;

namespace CrossPlanner.Domain.OtherModels
{
    public class DailyScheduleViewModel
    {
        public DateTime SelectedDate { get; set; }

        public List<ScheduledClass> ScheduledClasses {  get; set; }  = new List<ScheduledClass>();

        public List<ScheduledClassDetail> ScheduledClassDetails { get; set; } = new List<ScheduledClassDetail>();
    }
}