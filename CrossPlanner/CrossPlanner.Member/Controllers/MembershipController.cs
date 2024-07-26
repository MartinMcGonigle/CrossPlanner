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
    [Authorize(Roles = "SuperUser")]
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
            Steps.AddLast(new StepViewModel { Name = nameof(Finish), Title = "Finish", IncludeInReview = false });
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
                var membership = GetMembershipByMembershipIdAndMemberId(membershipId);

                if (membership == null)
                {
                    _logger.LogInformation($"{logPrefix} - Could not find membership with id {membershipId} for user with id: {_userId}");
                    return View("Error");
                }

                // Setup Stripe configuration
                StripeConfiguration.ApiKey = _stripeSettings.Value.SecretKey;

                Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
                var affiliate = GetAffiliateById(affiliateId);
                string connectedAccountId = affiliate.ConnectedAccountId;

                Customer customer = await _stripeService.GetOrCreateCustomer(User.FindFirst(ClaimTypes.Email)?.Value, stripeToken, connectedAccountId);

                var chargeOptions = new ChargeCreateOptions
                {
                    Amount = 500,
                    Currency = "gbp",
                    Description = "Membership Fee",
                    Customer = customer.Id,
                };

                var requestOptions = new RequestOptions
                {
                    StripeAccount = connectedAccountId
                };

                var service = new ChargeService();
                Charge charge = await service.CreateAsync(chargeOptions, requestOptions);

                if (charge.Paid)
                {
                    membership.IsActive = true;
                    membership.LastPaymentId = charge.Id;
                    membership.PaymentStatus = PaymentStatus.Completed;

                    _repositoryWrapper.MembershipRepository.Update(membership);
                    _repositoryWrapper.Save();

                    return RedirectToAction(NextStep.Name);
                }
                else
                {
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
    }
}