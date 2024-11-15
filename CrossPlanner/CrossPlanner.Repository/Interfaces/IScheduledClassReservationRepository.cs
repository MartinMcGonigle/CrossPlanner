﻿using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;

namespace CrossPlanner.Repository.Interfaces
{
    public interface IScheduledClassReservationRepository : IRepositoryBase<ScheduledClassReservation>
    {
        public int GetMemberClassAttendanceByMembershipId(int membershipId);

        public int GetMemberClassAttendanceByWeek(int membershipId, DateTime startOfWeek, DateTime endOfWeek);

        public List<ClassAttendeeViewModel> GetClassAttendeesByScheduledClassId(int scheduledClassId);

        public void DeleteFutureScheduledClassReservations(int membershipId, DateTime dateTime);

        public void UpdateFutureScheduledClassReservationMembershipId(int oldMembershipId, int newMembershipId, DateTime startDateTime);
    }
}