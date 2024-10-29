using CrossPlanner.Domain.Models;

namespace CrossPlanner.Domain.OtherModels
{
    public class AttendanceHistoryViewModel
    {
        public int AttendanceHistoryWeekly { get; set; }

        public int AttendanceHistoryMonthly { get; set; }

        public List<ScheduledClassReservation>? ScheduledClassReservations { get; set; }
    }
}