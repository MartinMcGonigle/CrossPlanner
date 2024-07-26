using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;

namespace CrossPlanner.Repository.Repository
{
    public class MembershipRepository : RepositoryBase<Membership>, IMembershipRepository
    {
        public MembershipRepository(ApplicationContext applicationContext) : base(applicationContext)
        {

        }
    }
}