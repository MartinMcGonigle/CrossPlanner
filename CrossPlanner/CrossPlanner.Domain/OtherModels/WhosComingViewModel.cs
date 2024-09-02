namespace CrossPlanner.Domain.OtherModels
{
    public class WhosComingViewModel
    {
        public int ScheduledClassId { get; set; }

        public string ClassName { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public List<ClassAttendeeViewModel> Attendees { get; set; } = new List<ClassAttendeeViewModel>();
    }
}