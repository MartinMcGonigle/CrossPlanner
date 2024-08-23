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

        public IEnumerable<AffiliateUsersViewModel> GetAffiliateUsers(string q, int affiliateId, int page, int pageSize, string linkedToGymSearch, string emailConfirmedSearch, string activeMembershipSearch, string roleSearch)
        {
            var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString);

            try
            {
                connection.Open();
                var parameters = new DynamicParameters();
                parameters.Add("search", q ?? "");
                parameters.Add("affiliateId", affiliateId);
                parameters.Add("page", page);
                parameters.Add("pageSize", pageSize);
                parameters.Add("linkedToGymSearch", linkedToGymSearch);
                parameters.Add("emailConfirmedSearch", emailConfirmedSearch);
                parameters.Add("activeMembershipSearch", activeMembershipSearch);
                parameters.Add("roleSearch", roleSearch);
                var data = connection.Query<AffiliateUsersViewModel>("spSelectAffiliateUsers", parameters, commandType: CommandType.StoredProcedure);

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

        public int GetAffiliateUsersCount(string q, int affiliateId, string linkedToGymSearch, string emailConfirmedSearch, string activeMembershipSearch, string roleSearch)
        {
            var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString);

            try
            {
                connection.Open();
                var parameters = new DynamicParameters();
                parameters.Add("search", q ?? "");
                parameters.Add("affiliateId", affiliateId);
                parameters.Add("linkedToGymSearch", linkedToGymSearch);
                parameters.Add("emailConfirmedSearch", emailConfirmedSearch);
                parameters.Add("activeMembershipSearch", activeMembershipSearch);
                parameters.Add("roleSearch", roleSearch);
                var data = connection.QuerySingle<int>("spSelectAffiliateUsersCount", parameters, commandType: CommandType.StoredProcedure);

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

        public IEnumerable<ApplicationUser> GetAffiliateActiveStaff(int affiliateId)
        {
            var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString);

            try
            {
                connection.Open();
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("affiliateId", affiliateId);
                var data = connection.Query<ApplicationUser>("spSelectAffiliateActiveStaff", dynamicParameters, commandType: CommandType.StoredProcedure);

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
    }
}