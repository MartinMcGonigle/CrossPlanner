using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CrossPlanner.Repository.Repository
{
    public class ApplicationUserRepository : RepositoryBase<ApplicationUser>, IApplicationUserRepository
    {
        public ApplicationUserRepository(ApplicationContext applicationContext) : base(applicationContext)
        {
        }

        public IEnumerable<ApplicationUser> GetAffiliateStaff(int affiliateId)
        {
            var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString);

            try
            {
                connection.Open();
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("AffiliateId", affiliateId);

                var data = connection.Query<ApplicationUser>("spSelectAffiliateStaff", dynamicParameters, commandType: CommandType.StoredProcedure);
                return data;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetDeactivatedUsers()
        {
            var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString);

            try
            {
                await connection.OpenAsync();
                var data = await connection.QueryAsync<ApplicationUser>("spSelectDeactivatedAccounts", commandType: CommandType.StoredProcedure);

                return data;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}