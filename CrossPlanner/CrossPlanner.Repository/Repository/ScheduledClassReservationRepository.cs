﻿using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
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
    }
}