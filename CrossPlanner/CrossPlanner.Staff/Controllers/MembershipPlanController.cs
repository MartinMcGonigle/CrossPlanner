using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrossPlanner.Staff.Controllers
{
    [Authorize(Roles = "SuperUser")]
    public class MembershipPlanController : Controller
    {
        private readonly ILogger<MembershipPlanController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private const string logPrefix = "Ctlr|MembershipPlan";

        public MembershipPlanController(
            ILogger<MembershipPlanController> logger,
            IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            _logger.LogInformation($"{logPrefix} - Attempting to display membership plans of affiliate with id {affiliateId}");

            var affiliateMembershipPlans = _repositoryWrapper.MembershipPlanRepository.GetAffiliateMembershipPlans(affiliateId);
            return View(affiliateMembershipPlans);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(MembershipPlan model)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            model.AffiliateId = affiliateId;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _logger.LogInformation($"{logPrefix} - Attempting to create membership plan for affiliate with id {affiliateId}");
                _repositoryWrapper.MembershipPlanRepository.Create(model);
                _repositoryWrapper.Save();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - Error creating membership plan: {ex.Message}");
                ModelState.AddModelError("", "Error creating membership plan: Please try again or contact support.");
                return View(model);
            }
        }
    }
}