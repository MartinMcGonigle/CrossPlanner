using CrossPlanner.Domain.Enums;
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

        [HttpGet]
        public IActionResult Edit(int membershipPlanId)
        {
            var membershipPlan = GetMembershipPlanById(membershipPlanId);

            if (membershipPlan == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit membership plan with id {membershipPlanId} was not found.");
                TempData["ErrorMessage"] = "Membership plan not found.";
                return RedirectToAction("Index");
            }

            return View(membershipPlan);
        }

        [HttpPost]
        public IActionResult Edit(MembershipPlan model)
        {
            var membershipPlan = GetMembershipPlanById(model.MembershipPlanId);

            if (membershipPlan == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit membership plan with id {model.MembershipPlanId} was not found.");
                TempData["ErrorMessage"] = "Membership plan not found.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _logger.LogInformation($"{logPrefix} - Attempting to edit membership plan with id {membershipPlan.MembershipPlanId}");

                membershipPlan.Title = model.Title;
                membershipPlan.Price = model.Price;
                membershipPlan.Description = model.Description;
                membershipPlan.Type = model.Type;
                membershipPlan.NumberOfClasses = model.NumberOfClasses;
                membershipPlan.NumberOfMonths = model.NumberOfMonths;

                _repositoryWrapper.MembershipPlanRepository.Update(membershipPlan);
                _repositoryWrapper.Save();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - Error editing membership plan: {ex}");
                ModelState.AddModelError("", "Error updating membership plan: Please try again or contact support.");
                return View(model);
            }
        }

        [HttpPut]
        public async Task<IActionResult> ToggleMembershipPlan(int membershipPlanId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to toggle membership plan with id {membershipPlanId}");

            var membershipPlan = GetMembershipPlanById(membershipPlanId);
            if (membershipPlan == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to toggle membership plan with id {membershipPlanId} as plan could not be found.");
                return Json(new { message = "Membership plan not found." });
            }

            try
            {
                membershipPlan.IsActive = !membershipPlan.IsActive;
                _repositoryWrapper.MembershipPlanRepository.Update(membershipPlan);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Membership plan toggled successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to toggle membership plan with id {membershipPlanId}: {ex}");
                return Json(new { message = "An error occurred while toggling membership plan." });
            }
        }

        private MembershipPlan GetMembershipPlanById(int id)
        {
            return _repositoryWrapper.MembershipPlanRepository
                .FindByCondition(x => x.MembershipPlanId == id)
                .FirstOrDefault();
        }
    }
}