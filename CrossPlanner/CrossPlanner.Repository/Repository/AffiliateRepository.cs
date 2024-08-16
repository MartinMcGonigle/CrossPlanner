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
    public class AffiliateRepository : RepositoryBase<Affiliate>, IAffiliateRepository
    {
        public AffiliateRepository(ApplicationContext applicationContext) : base(applicationContext)
        {
        }

        public IEnumerable<AffiliateMembersViewModel> GetAffiliateMembers(string q, int affiliateId, int page, int pageSize)
        {
            var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString);

            try
            {
                connection.Open();
                var parameters = new DynamicParameters();
                parameters.Add("affiliateId", affiliateId);
                parameters.Add("search", q ?? "");
                parameters.Add("page", page);
                parameters.Add("pageSize", pageSize);
                var data = connection.Query<AffiliateMembersViewModel>("spSelectAffiliateMembers", parameters, commandType: CommandType.StoredProcedure);

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



        public int GetAffiliateMembersCount(string q, int affiliateId)
        {
            var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString);

            try
            {
                connection.Open();
                var parameters = new DynamicParameters();
                parameters.Add("affiliateId", affiliateId);
                parameters.Add("search", q ?? "");
                var data = connection.QuerySingle<int>("spSelectAffiliateMembersCount", parameters, commandType: CommandType.StoredProcedure);

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
    }
}