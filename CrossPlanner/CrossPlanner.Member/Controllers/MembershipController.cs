using CrossPlanner.Domain.Enums;
using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;
using CrossPlanner.Repository.Wrapper;
using CrossPlanner.Service.Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using System.Security.Claims;

namespace CrossPlanner.Member.Controllers
{
    [Authorize(Roles = "Member")]
    public class MembershipController : BaseController
    {
        private readonly ILogger<MembershipController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IStripeService _stripeService;
        private readonly IOptions<StripeSettings> _stripeSettings;
        private const string logPrefix = "Ctlr|Membership";

        public MembershipController(
            ILogger<MembershipController> logger,
            IRepositoryWrapper repositoryWrapper,
            IHttpContextAccessor httpContextAccessor,
            IStripeService stripeService,
            IOptions<StripeSettings> stripeSettings)
            : base(httpContextAccessor)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _stripeService = stripeService;
            _stripeSettings = stripeSettings;

            Steps = new LinkedList<StepViewModel>();
            Steps.AddLast(new StepViewModel { Name = nameof(MembershipSelection), Title = "Select Membership" });
            Steps.AddLast(new StepViewModel { Name = nameof(MembershipDetails), Title = "Membership Details" });
            Steps.AddLast(new StepViewModel { Name = nameof(Review), Title = "Review" });
            Steps.AddLast(new StepViewModel { Name = nameof(PaymentDetails), Title = "Payment Details" });
            Steps.AddLast(new StepViewModel { Name = nameof(Finish), Title = "Finish" });
        }

        [HttpGet("membership-selection")]
        public IActionResult MembershipSelection(int membershipId)
        {
            try
            {
                var existingMembership = CheckForExistingMembership();

                if (existingMembership != null)
                {
                    _logger.LogWarning($"{logPrefix} - {_userId} already has an active membership.");
                    return RedirectToAction(nameof(ExistingMembership));
                }

                var membership = _repositoryWrapper.MembershipRepository
                    .FindByCondition(m => m.MembershipId == membershipId && m.MemberId == _userId)
                    .FirstOrDefault() ?? new Membership();

                Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
                var membershipPlans = GetAffiliateMembershipPlans(affiliateId);

                var viewModel = new MembershipSelectionViewModel
                {
                    MembershipPlans = membershipPlans,
                    Membership = membership
                };

                ViewData["HeaderType"] = "Select Membership";
                ViewData["MembershipId"] = membershipId;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - There was an error in [HttpGet] MembershipSelection: {ex}");
                return View("Error");
            }
        }

        [HttpPost("membership-selection")]
        public IActionResult MembershipSelection(MembershipSelectionViewModel model)
        {
            if (model.Membership == null || model.Membership.MembershipPlanId == 0)
            {
                _logger.LogError($"{logPrefix} - {_userId} is trying to purchase membership but either model.Membership is null or model.Membership.MembershipPlanId is 0.");
                return View("Error");
            }

            try
            {
                var membershipPlan = GetMembershipPlan(model.Membership.MembershipPlanId);

                if (membershipPlan.Type == (int)MembershipType.Monthly)
                {
                    model.Membership.EndDate = DateTime.Now.AddMonths((int)membershipPlan.NumberOfMonths);
                } else if (membershipPlan.Type == (int)MembershipType.Weekly || membershipPlan.Type == (int)MembershipType.Unlimited)
                {
                    model.Membership.EndDate = DateTime.Now.AddMonths(1);
                }
                
                model.Membership.StartDate = DateTime.Now;
                model.Membership.MemberId = _userId;

                if (model.Membership.MembershipId == 0)
                {
                    _repositoryWrapper.MembershipRepository.Create(model.Membership);
                }
                else
                {
                    _repositoryWrapper.MembershipRepository.Update(model.Membership);
                }

                _repositoryWrapper.Save();
                return RedirectToAction(NextStep.Name, new { membershipId = model.Membership.MembershipId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - There was an error in [HttpPost] MembershipSelection: {ex}");
                return View("Error");
            }
        }

        [HttpGet("membership-details")]
        public IActionResult MembershipDetails(int membershipId)
        {
            try
            {
                var membership = GetMembershipByMembershipIdAndMemberId(membershipId);

                if (membership == null)
                {
                    _logger.LogInformation($"{logPrefix} - Could not find membership with id {membershipId} for user with id: {_userId}");
                    return View("Error");
                }

                ViewData["HeaderType"] = "Membership Details";
                ViewData["MembershipId"] = membershipId;

                return View(membership);
            }
            catch(Exception ex)
            {
                _logger.LogError($"{logPrefix} - There was an error in [HttpGet] MembershipDetails: {ex}");
                return View("Error");
            }
        }

        [HttpPost("membership-details")]
        public IActionResult MembershipDetails(Membership model)
        {
            try
            {
                var membership = GetMembershipByMembershipIdAndMemberId(model.MembershipId);

                if (membership == null)
                {
                    _logger.LogInformation($"{logPrefix} - Could not find membership with id {model.MembershipId} for user with id: {_userId}");
                    return View("Error");
                }

                membership.AutoRenew = model.AutoRenew;

                _repositoryWrapper.MembershipRepository.Update(membership);
                _repositoryWrapper.Save();

                return RedirectToAction(NextStep.Name, new { membershipId = model.MembershipId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - There was an error in [HttpPost] MembershipDetails: {ex}");
                return View("Error");
            }
        }

        [HttpGet("review")]
        public IActionResult Review(int membershipId)
        {
            try
            {
                var membership = _repositoryWrapper.MembershipRepository
                    .FindByCondition(m => m.MembershipId == membershipId && m.MemberId == _userId)
                    .Include("MembershipPlan")
                    .FirstOrDefault();

                if (membership == null)
                {
                    _logger.LogInformation($"{logPrefix} - Could not find membership with id {membershipId} for user with id: {_userId}");
                    return View("Error");
                }

                ViewData["HeaderType"] = "Review";
                ViewData["MembershipId"] = membershipId;

                return View(membership);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - There was an error in [HttpGet] Review: {ex}");
                return View("Error");
            }
        }

        [HttpGet("payment-details")]
        public IActionResult PaymentDetails(int membershipId)
        {
            try
            {
                var membership = GetMembershipByMembershipIdAndMemberId(membershipId);

                if (membership == null)
                {
                    _logger.LogInformation($"{logPrefix} - Could not find membership with id {membershipId} for user with id: {_userId}");
                    return View("Error");
                }

                ViewData["HeaderType"] = "Payment Details";
                ViewData["MembershipId"] = membershipId;

                return View(membership);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - There was an error in [HttpGet] PaymentDetails: {ex}");
                return View("Error");
            }
        }

        [HttpPost("payment-details")]
        public async Task<IActionResult> PaymentDetails(string stripeToken, int membershipId)
        {
            try
            {
                var membership = _repositoryWrapper.MembershipRepository
                    .FindByCondition(m => m.MembershipId == membershipId && m.MemberId == _userId)
                    .Include("MembershipPlan")
                    .FirstOrDefault();

                if (membership == null)
                {
                    _logger.LogError($"{logPrefix} - Could not find membership with id {membershipId} for user with id: {_userId}");
                    return View("Error");
                }

                Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
                var affiliate = GetAffiliateById(affiliateId);

                var customer = await _stripeService.GetOrCreateCustomer(User.FindFirst(ClaimTypes.Email)?.Value, stripeToken, affiliate.ConnectedAccountId);
                var processPayment = await _stripeService.ProcessPayment(membership.MembershipPlan.Price, customer.Id, affiliate.ConnectedAccountId);

                if (processPayment.Success)
                {
                    membership.IsActive = true;
                    membership.LastPaymentId = processPayment.PaymentId;
                    membership.PaymentStatus = PaymentStatus.Paid;
                    membership.LastPaymentAmount = membership.MembershipPlan.Price;

                    _repositoryWrapper.MembershipRepository.Update(membership);
                    _repositoryWrapper.Save();

                    StoreStripeCustomerId(customer.Id, affiliateId);

                    return RedirectToAction(NextStep.Name);
                }
                else
                {
                    _logger.LogError("Payment failed for user {UserId} with membership ID {MembershipId}. Stripe Charge ID: {ChargeId}", _userId, membershipId, processPayment.PaymentId);
                    TempData["ErrorMessage"] = "We couldn't process your payment. Please check your card details and try again. If the problem persists, contact our support team.";

                    ViewData["HeaderType"] = "Payment Details";
                    ViewData["MembershipId"] = membershipId;

                    return View(membership);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - There was an error in [HttpPost] PaymentDetails: {ex}");
                return View("Error");
            }
        }

        public IActionResult Finish()
        {
            return View();
        }

        private Membership CheckForExistingMembership()
        {
            var existingMembership = _repositoryWrapper.MembershipRepository
                .FindByCondition(m => m.MemberId == _userId && m.IsActive)
                .FirstOrDefault();

            return existingMembership;
        }

        [HttpGet]
        public IActionResult ExistingMembership()
        {
            return View();
        }

        private List<MembershipPlan> GetAffiliateMembershipPlans(int affiliateId)
        {
            return _repositoryWrapper.MembershipPlanRepository
                .FindAll()
                .Where(mp => mp.IsActive && mp.AffiliateId == affiliateId)
                .ToList();
        }

        private MembershipPlan GetMembershipPlan(int membershipPlanId)
        {
            return _repositoryWrapper.MembershipPlanRepository
                .FindByCondition(mp => mp.MembershipPlanId == membershipPlanId)
                .FirstOrDefault();
        }

        private Membership GetMembershipByMembershipIdAndMemberId(int membershipId)
        {
            return _repositoryWrapper.MembershipRepository
                .FindByCondition(m => m.MembershipId == membershipId && m.MemberId == _userId)
                .FirstOrDefault();
        }

        private Affiliate GetAffiliateById(int affiliateId)
        {
            return _repositoryWrapper.AffiliateRepository
                .FindByCondition(a => a.AffiliateId == affiliateId)
                .FirstOrDefault();
        }

        private void StoreStripeCustomerId(string stripeCustomerId, int affiliateId)
        {
            var userStripeAffiliate = _repositoryWrapper.UserStripeAffiliateRepository
                .FindByCondition(usa => usa.StripeCustomerId == stripeCustomerId
                && usa.ApplicationUserId == _userId
                && usa.AffiliateId == affiliateId)
                .FirstOrDefault();

            if (userStripeAffiliate != null)
                return;

            var newUserStripeAffiliate = new UserStripeAffiliate
            {
                StripeCustomerId = stripeCustomerId,
                ApplicationUserId = _userId,
                AffiliateId = affiliateId
            };

            _repositoryWrapper.UserStripeAffiliateRepository.Create(newUserStripeAffiliate);
            _repositoryWrapper.Save();
        }
    }
}