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
    public class ScheduledClassRepository : RepositoryBase<ScheduledClass>, IScheduledClassRepository
    {
        public ScheduledClassRepository(ApplicationContext applicationContext) : base (applicationContext)
        {
            
        }

        public List<ScheduledClass> GetAffiliateScheduledClassByDate(int affiliateId, DateTime dateTime)
        {
            var scheduledClasses = _applicationContext.ScheduledClasses
                .Where(sc =>
                sc.ClassType.AffiliateId == affiliateId &&
                sc.StartDateTime.Date == dateTime.Date &&
                !sc.IsDeleted)
                .Include(sc => sc.ClassType)
                .Include (sc => sc.Instructor)
                .OrderBy(sc => sc.StartDateTime)
                .AsNoTracking()
                .ToList();

            return scheduledClasses;
        }

        public IEnumerable<ScheduledClassDetail> GetAffiliateScheduledClassByDateMember(int affiliateId, DateTime dateTime, string memberId)
        {
            var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString);

            try
            {
                connection.Open();
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("affiliateId", affiliateId);
                dynamicParameters.Add("dateTime", dateTime);
                dynamicParameters.Add("memberId", memberId);
                var data = connection.Query<ScheduledClassDetail>("spSelectAffiliateScheduledClassByDateMember", dynamicParameters, commandType: CommandType.StoredProcedure);

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



        public ScheduledClass? GetScheduledClassById(int scheduledClassId)
        {
            using (var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString))
            {
                try
                {
                    connection.Open();
                    var dynamicParameters = new DynamicParameters();
                    dynamicParameters.Add("scheduledClassId", scheduledClassId);
                    var data = connection.QuerySingleOrDefault<ScheduledClass>("spSelectScheduledClassById", dynamicParameters, commandType: CommandType.StoredProcedure);

                    return data;
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred while retrieving the scheduled class.", ex);
                }
            }
        }

    }
}