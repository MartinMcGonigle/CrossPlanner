using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;
using CrossPlanner.Repository.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CrossPlanner.Repository.Repository
{
    public class ScheduledClassReservationRepository : RepositoryBase<ScheduledClassReservation>, IScheduledClassReservationRepository
    {
        public ScheduledClassReservationRepository(ApplicationContext applicationContext) : base (applicationContext)
        {
            
        }

        public int GetMemberClassAttendanceByMembershipId(int membershipId)
        {
            var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString);

            try
            {
                connection.Open();
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("membershipId", membershipId);
                var data = connection.QuerySingle<int>("spSelectMemberClassAttendanceByMembershipId", dynamicParameters, commandType: CommandType.StoredProcedure);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public int GetMemberClassAttendanceByWeek(int membershipId, DateTime startOfWeek, DateTime endOfWeek)
        {
            return _applicationContext.ScheduledClassReservations
                .Where(scr => scr.MembershipId == membershipId
                && scr.ScheduledClass.IsCancelled == false
                && (scr.IsPresent == true || scr.IsPresent == null)
                && scr.ScheduledClass.StartDateTime >= startOfWeek
                && scr.ScheduledClass.EndDateTime <= endOfWeek)
                .Count();
        }

        public List<ClassAttendeeViewModel> GetClassAttendeesByScheduledClassId(int scheduledClassId)
        {
            var attendees = _applicationContext.ScheduledClassReservations
                .Where(scr => scr.ScheduledClassId == scheduledClassId)
                .Include(scr => scr.Membership)
                .ThenInclude(m => m.Member)
                .Select(scr => new ClassAttendeeViewModel
                {
                    Name = $"{scr.Membership.Member.FirstName} {scr.Membership.Member.LastName}",
                    IsPresent = scr.IsPresent,
                    ScheduledClassReservationId = scr.ScheduledClassReservationId,
                    ProfilePictureUrl = scr.Membership.Member.ProfilePictureUrl
                })
                .ToList();

            return attendees;
        }

        public void DeleteFutureScheduledClassReservations(int membershipId, DateTime dateTime)
        {
            using (var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString))
            {
                try
                {
                    connection.Open();
                    var dynamicParameters = new DynamicParameters();
                    dynamicParameters.Add("membershipId", membershipId);
                    dynamicParameters.Add("startDateTime", dateTime);
                    connection.Execute("spDeleteFutureScheduledClassReservation", dynamicParameters, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error executing spDeleteFutureScheduledClassReservation", ex);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public void UpdateFutureScheduledClassReservationMembershipId(int oldMembershipId, int newMembershipId, DateTime startDateTime)
        {
            using (var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString))
            {
                try
                {
                    connection.Open();
                    var dynamicParameters = new DynamicParameters();
                    dynamicParameters.Add("oldMembershipId", oldMembershipId);
                    dynamicParameters.Add("newMembershipId", newMembershipId);
                    dynamicParameters.Add("startDateTime", startDateTime);

                    connection.Execute("spUpdateFutureScheduledClassReservationMembershipId", dynamicParameters, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error executing spUpdateFutureScheduledClassReservationMembershipId", ex);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}