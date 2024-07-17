using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrossPlanner.Staff.Controllers
{
    [Authorize(Roles = "SuperUser")]
    public class MemberController : BaseController
    {
        private readonly ILogger<MemberController> _memberLogger;
        private const string memberLogPrefix = "Ctlr|Member";

        public MemberController(
            UserManager<ApplicationUser> userManager,
            IRepositoryWrapper repositoryWrapper,
            IEmailSender emailSender,
            ILogger<MemberController> logger,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager)
            : base (userManager, repositoryWrapper, emailSender, logger, configuration, roleManager)
        {
            _memberLogger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            _memberLogger.LogInformation($"{memberLogPrefix} - Attempting to display members of affiliate with id {affiliateId}");

            var affiliateMembers = _repositoryWrapper.ApplicationUserRepository.GetAffiliateMembers(affiliateId);

            return View(affiliateMembers);
        }
    }
}
