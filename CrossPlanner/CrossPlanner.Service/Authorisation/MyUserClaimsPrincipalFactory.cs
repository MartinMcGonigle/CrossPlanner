using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace CrossPlanner.Service.Authorisation
{
    public class MyUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        
        public MyUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> options,
            IRepositoryWrapper repositoryWrapper)
            : base(userManager, roleManager, options)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var affiliateUser = _repositoryWrapper.AffiliateUsersRepository
                .FindByCondition(x => x.ApplicationUserId == user.Id && x.IsActive)
                .FirstOrDefault();

            var identity = await base.GenerateClaimsAsync (user);
            identity.AddClaim(new Claim("Affiliate", affiliateUser?.AffiliateId.ToString() ?? ""));

            return identity;
        }
    }
}