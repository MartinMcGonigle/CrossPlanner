using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CrossPlanner.Repository.Repository
{
    public class MembershipPlanRepository : RepositoryBase<MembershipPlan>, IMembershipPlanRepository
    {
        public MembershipPlanRepository(ApplicationContext applicationContext) : base(applicationContext)
        {
            
        }

        public IEnumerable<MembershipPlan> GetAffiliateMembershipPlans(int affiliateId)
        {
            var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString);

            try
            {
                connection.Open();
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("AffiliateId", affiliateId);
                
                var data = connection.Query<MembershipPlan>("spSelectAffiliateMembershipPlans", dynamicParameters, commandType: CommandType.StoredProcedure);
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