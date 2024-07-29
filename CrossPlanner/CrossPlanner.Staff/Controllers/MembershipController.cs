using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CrossPlanner.Staff.Controllers
{
    [Authorize(Roles = "SuperUser")]
    public class MembershipController : Controller
    {
        private readonly ILogger<MembershipController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private const string logPrefix = "Ctlr|Membership";

        public MembershipController(
            ILogger<MembershipController> logger,
            IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
        }

        [HttpGet]
        public IActionResult Index(string q, int page = 1, int pageSize = 25)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            _logger.LogInformation($"{logPrefix} - Displaying memberships for affiliate with id {affiliateId}");

            var data = _repositoryWrapper.MembershipRepository.GetAffiliateMemberships(q, affiliateId, page, pageSize);
            var count = _repositoryWrapper.MembershipRepository.GetAffiliateMembershipsCount(q, affiliateId, page, pageSize);

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
