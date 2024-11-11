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
    [Authorize(Roles = "SuperUser,Manager")]
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
        public IActionResult ViewMembership(string applicationUserId)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            _logger.LogInformation($"{logPrefix} - Displaying memberships for member with id {applicationUserId} for affiliate with id {affiliateId}");

            var data = _repositoryWrapper.MembershipRepository.GetUserMemberships(affiliateId, applicationUserId);

            return View(data);
        }

        [HttpPut]
        public async Task<IActionResult> CancelMembership(int membershipId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to cancel membership with id {membershipId}");

            using (var transaction = _repositoryWrapper.BeginTransaction())
            {
                try
                {
                    var membership = _repositoryWrapper.MembershipRepository
                        .FindByCondition(m => m.MembershipId == membershipId)
                        .Include("MembershipPlan")
                        .FirstOrDefault();

                    if (membership == null || string.IsNullOrEmpty(membership.LastPaymentId))
                    {
                        _logger.LogWarning($"{logPrefix} - Unable to cancel membership with id {membershipId} as could not be found");
                        return Json(new { message = "Membership could not be found." });
                    }

                    Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
                    var affiliate = GetAffiliateById(affiliateId);

                    if (affiliate == null)
                    {
                        _logger.LogError($"{logPrefix} - Could not find affiliate with id {affiliateId}");
                        return Json(new { message = "Unable to cancel membership." });
                    }

                    if (string.IsNullOrEmpty(affiliate.ConnectedAccountId))
                    {
                        _logger.LogError($"{logPrefix} - Affiliate with id {affiliateId} does not have ConnectedAccountId.");
                        return Json(new { message = "Unable to cancel membership." });
                    }

                    decimal refundAmount = CalculateRefundAmount(membership);
                    var processRefund = await _stripeService.RefundCustomer(refundAmount, membership.LastPaymentId, affiliate.ConnectedAccountId);

                    if (processRefund.Success)
                    {
                        membership.IsActive = false;
                        membership.AutoRenew = false;
                        membership.PaymentStatus = PaymentStatus.Refunded;
                        _repositoryWrapper.MembershipRepository.Update(membership);

                        var refund = new Refund
                        {
                            MembershipId = membership.MembershipId,
                            Amount = refundAmount,
                            RefundDate = DateTime.Now,
                            StripeRefundId =processRefund.RefundId,
                        };

                        _repositoryWrapper.RefundRepository.Create(refund);
                        _repositoryWrapper.Save();
                        transaction.Commit();

                        _logger.LogInformation($"{logPrefix} - Membership with id {membershipId} has been successfully canceled");
                        return Json(new { message = "Membership canceled." });
                    }
                    else
                    {
                        _logger.LogError($"{logPrefix} - Failed to cancel membership with id {membershipId }");
                        return Json(new { message = "Unable to cancel membership." });
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError($"{logPrefix} - An error occurred while attempting to cancel membership with id {membershipId}: {ex}");
                    return Json(new { message = "An error occurred while canceling membership." });
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
                decimal weeklyCost = (decimal)(model.LastPaymentAmount / totalWeeks);

                return weeklyCost * weeksLeft;
            }
            else if (model.MembershipPlan.Type == (int)MembershipType.Unlimited)
            {
                // Refund is calculated based on the total days
                int totalDays = (model.EndDate ?? DateTime.Now).Subtract(model.StartDate).Days;
                int usedDays = DateTime.Now.Subtract(model.StartDate).Days;
                int unusedDays = totalDays - usedDays;
                decimal dailyRate = (decimal)(model.LastPaymentAmount / totalDays);

                return dailyRate * unusedDays;
            }
            else
            {
                _logger.LogWarning($"{logPrefix} - Encountered an unrecognized membership type for membership with id: {model.MembershipId}");
                return 0;
            }
        }

        // Maybe implement the below at a later date

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

        private Affiliate GetAffiliateById(int affiliateId)
        {
            return _repositoryWrapper.AffiliateRepository
                .FindByCondition(a => a.AffiliateId == affiliateId)
                .FirstOrDefault();
        }
    }
}