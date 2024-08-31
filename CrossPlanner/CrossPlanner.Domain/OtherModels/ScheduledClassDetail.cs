using System.ComponentModel.DataAnnotations;

namespace CrossPlanner.Domain.OtherModels
{
    public class ScheduledClassDetail
    {
        public int ScheduledClassId { get; set; }

        public string ClassTypeTitle { get; set; }

        public string InstructorFirstName { get; set; }

        public string InstructorLastName { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public int? Capacity { get; set; }

        public bool IsActive { get; set; }

        public bool IsCancelled { get; set; }

        public string? CancellationReason { get; set; }

        public bool IsDeleted { get; set; }

        public int ReservationsCount { get; set; }

        public bool Reserved { get; set; } = false;

        public int ScheduledClassReservationId { get; set; }
    }
}