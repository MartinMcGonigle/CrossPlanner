using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrossPlanner.Domain.Models;
using Microsoft.EntityFrameworkCore;
using CrossPlanner.Domain.Enums;
using CrossPlanner.Domain.OtherModels;
using Microsoft.Extensions.Options;
using CrossPlanner.Service.Stripe;

namespace CrossPlanner.Staff.Controllers
{
    [Authorize(Roles = "SuperUser")]
    public class MembershipController : Controller
    {
        private readonly ILogger<MembershipController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IOptions<StripeSettings> _stripeSettings;
        private readonly IStripeService _stripeService;
        private const string logPrefix = "Ctlr|Membership";

        public MembershipController(
            ILogger<MembershipController> logger,
            IRepositoryWrapper repositoryWrapper,
            IOptions<StripeSettings> stripeSettings,
            IStripeService stripeService)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _stripeSettings = stripeSettings;
            _stripeService = stripeService;
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

        [HttpGet]
        public IActionResult ManageMembership(string memberId)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            _logger.LogInformation($"{logPrefix} - Displaying memberships for member with id {memberId} for affiliate with id {affiliateId}");

            var data = _repositoryWrapper.MembershipRepository.GetUserMemberships(affiliateId, memberId);

            return View(data);
        }

        [HttpPut]
        public async Task<IActionResult> DeactivateMembership(int membershipId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to deactivate membership with id {membershipId}");

            using (var transaction = _repositoryWrapper.BeginTransaction())
            {
                try
                {
                    var membership = _repositoryWrapper.MembershipRepository
                        .FindByCondition(m => m.MembershipId == membershipId)
                        .Include("MembershipPlan")
                        .FirstOrDefault();

                    if (membership == null)
                    {
                        _logger.LogWarning($"{logPrefix} - Unable to deactivate membership with id {membershipId} as could not be found");
                        return Json(new { message = "Membership could not be found." });
                    }
                    
                    decimal refundAmount = CalculateRefundAmount(membership);
                    var (success, message) = await _stripeService.RefundCustomer(refundAmount, membership.LastPaymentId);

                    if (success)
                    {
                        membership.IsActive = false;
                        membership.PaymentStatus = PaymentStatus.Refunded;
                        _repositoryWrapper.MembershipRepository.Update(membership);

                        var refund = new Refund
                        {
                            MembershipId = membership.MembershipId,
                            Amount = refundAmount,
                            RefundDate = DateTime.Now
                        };

                        _repositoryWrapper.RefundRepository.Create(refund);
                        _repositoryWrapper.Save();
                        transaction.Commit();

                        _logger.LogInformation($"{logPrefix} - Membership with ID: {membershipId} has been successfully deactivated");
                        return Json(new { message = "Membership deactivated." });
                    }

                    _logger.LogWarning($"{logPrefix} - Failed to deactivate membership with id {membershipId}: {message}");
                    return Json(new { message = "Membership could not be deactivated. " + message });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError($"{logPrefix} - An error occurred while attempting to deactivate membership with id {membershipId}: {ex}");
                    return Json(new { message = "An error occurred while deactivating membership." });
                }
            }
        }

        private decimal CalculateRefundAmount(Membership model)
        {
            if (model.MembershipPlan.Type == (int)MembershipType.Monthly)
            {
                /*
                    * Calculate the total number of classes allowed over the entire membership period
                    * Determine how many classes have been attended
                    * Calculate the cost per class by dividing the total payment by the total number of classes
                    * Refund the cost for the number of unattended classes
                 */

                return 0;
            }
            else if (model.MembershipPlan.Type == (int)MembershipType.Weekly)
            {
                // Refund is calculated based on the weeks remaining in the membership after cancellation
                int totalWeeks = (model.EndDate ?? DateTime.Now).Subtract(model.StartDate).Days / 7;
                int weeksUsed = DateTime.Now.Subtract(model.StartDate).Days / 7;
                int weeksLeft = totalWeeks - weeksUsed;
                decimal weeklyCost = model.LastPaymentAmount / totalWeeks;

                return weeklyCost * weeksLeft;
            }
            else if (model.MembershipPlan.Type == (int)MembershipType.Unlimited)
            {
                // Refund is calculated based on the total days
                int totalDays = (model.EndDate ?? DateTime.Now).Subtract(model.StartDate).Days;
                int usedDays = DateTime.Now.Subtract(model.StartDate).Days;
                int unusedDays = totalDays - usedDays;
                decimal dailyRate = model.LastPaymentAmount / totalDays;

                return dailyRate * unusedDays;
            }
            else
            {
                _logger.LogWarning($"{logPrefix} - Encountered an unrecognized membership type for membership with id: {model.MembershipId}");
                return 0;
            }
        }
    }
}