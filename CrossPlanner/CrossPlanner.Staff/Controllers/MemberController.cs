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
        public IActionResult Index(string q, int page = 1, int pageSize = 25)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            _memberLogger.LogInformation($"{memberLogPrefix} - Attempting to display members of affiliate with id {affiliateId}");

            var data = _repositoryWrapper.AffiliateRepository.GetAffiliateMembers(q, affiliateId, page, pageSize);
            var count = _repositoryWrapper.AffiliateRepository.GetAffiliateMembersCount(q, affiliateId);

            ViewData["CurrentFilter"] = q;

            // Paging
            ViewData["Page"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["RecordCount"] = count;
            ViewData["Action"] = "Index";

            return View(data);
        }
    }
}
