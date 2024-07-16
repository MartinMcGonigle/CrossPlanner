using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CrossPlanner.Staff.Controllers
{
    [Authorize(Roles = "SuperUser")]
    public class StaffController : BaseController
    {
        private readonly ILogger<StaffController> _staffLogger;
        private const string staffLogPrefix = "Ctlr|Staff";

        public StaffController(
            UserManager<ApplicationUser> userManager,
            IRepositoryWrapper repositoryWrapper,
            IEmailSender emailSender,
            ILogger<StaffController> logger,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager)
            : base(userManager, repositoryWrapper, emailSender, logger, configuration, roleManager)
        {
            _staffLogger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            _staffLogger.LogInformation($"{staffLogPrefix} - Attempting to display staff members of affiliate with id {affiliateId}");

            var affiliateStaff = _repositoryWrapper.ApplicationUserRepository.GetAffiliateStaff(affiliateId);

            return View(affiliateStaff);
        }
    }
}