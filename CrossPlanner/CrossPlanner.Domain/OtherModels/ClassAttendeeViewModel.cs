namespace CrossPlanner.Domain.OtherModels
{
    public class ClassAttendeeViewModel
    {
        public string Name { get; set; }

        public string ProfilePictureUrl { get; set; } = "/images/default-profile.png";

        public bool? IsPresent { get; set; }

        public int ScheduledClassReservationId { get; set; }
    }
}