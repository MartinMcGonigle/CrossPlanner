using CrossPlanner.Domain.Models;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IScheduledClassReservationRepository : IRepositoryBase<ScheduledClassReservation>
    {
        public int GetMemberClassAttendanceByMembershipId(int membershipId);

        public int GetMemberClassAttendanceByWeek(int membershipId, DateTime startOfWeek, DateTime endOfWeek);
    }
}