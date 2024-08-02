using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Enums;
using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;
using CrossPlanner.Repository.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CrossPlanner.Repository.Repository
{
    public class MembershipRepository : RepositoryBase<Membership>, IMembershipRepository
    {
        public MembershipRepository(ApplicationContext applicationContext) : base(applicationContext)
        {

        }

        public IEnumerable<MembershipViewModel> GetAffiliateMemberships(string q, int affiliateId, int page, int pageSize)
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
                var data = connection.Query<MembershipViewModel>("spSelectAffiliateMemberships", parameters, commandType: CommandType.StoredProcedure);

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

        public int GetAffiliateMembershipsCount(string q, int affiliateId, int page, int pageSize)
        {
            var connection = new SqlConnection(_applicationContext.Database.GetDbConnection().ConnectionString);

            try
            {
                connection.Open();
                var parameters = new DynamicParameters();
                parameters.Add("affiliateId", affiliateId);
                parameters.Add("search", q ?? "");
                // parameters.Add("page", page);
                // parameters.Add("pageSize", pageSize);
                var data = connection.QuerySingle<int>("spSelectAffiliateMembershipsCount", parameters, commandType: CommandType.StoredProcedure);

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


        public IEnumerable<Membership> GetUserMemberships(int affiliateId, string memberId)
        {
            return _applicationContext.Memberships
                .Where(m => m.MemberId == memberId
                && m.PaymentStatus == PaymentStatus.Paid
                && m.MembershipPlan.AffiliateId == affiliateId)
                .Include(m => m.MembershipPlan)
                .OrderByDescending(m => m.StartDate )
                .ToList();
        }
    }
}