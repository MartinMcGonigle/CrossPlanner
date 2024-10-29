using CrossPlanner.Domain.Enums;
using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;
using CrossPlanner.Repository.Wrapper;
using CrossPlanner.Service.Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private const string logPrefix = "Ctlr|Membership";

        public MembershipController(
            ILogger<MembershipController> logger,
            IRepositoryWrapper repositoryWrapper,
            IHttpContextAccessor httpContextAccessor,
            IStripeService stripeService,
            IOptions<StripeSettings> stripeSettings,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
            : base(httpContextAccessor)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _stripeService = stripeService;
            _stripeSettings = stripeSettings;
            _signInManager = signInManager;
            _emailSender = emailSender;

            Steps = new LinkedList<StepViewModel>();
            Steps.AddLast(new StepViewModel { Name = nameof(MembershipSelection), Title = "Select Membership" });
            Steps.AddLast(new StepViewModel { Name = nameof(MembershipDetails), Title = "Membership Details" });
            Steps.AddLast(new StepViewModel { Name = nameof(Review), Title = "Review" });
            Steps.AddLast(new StepViewModel { Name = nameof(PaymentDetails), Title = "Payment Details" });
            Steps.AddLast(new StepViewModel { Name = nameof(Finish), Title = "Finish" });
        }

        [HttpGet("membership-selection")]
        public async Task<IActionResult> MembershipSelection(int membershipId)
        {
            try
            {
                Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (affiliateId == 0 || string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId} or userId is missing");
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                var existingMembership = CheckForExistingMembership();

                if (existingMembership != null)
                {
                    _logger.LogWarning($"{logPrefix} - {_userId} already has an active membership.");
                    return RedirectToAction(nameof(ExistingMembership));
                }

                var membership = _repositoryWrapper.MembershipRepository
                    .FindByCondition(m => m.MembershipId == membershipId && m.MemberId == _userId)
                    .FirstOrDefault() ?? new Membership();

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
                var existingMembership = CheckForExistingMembership();

                if (existingMembership != null)
                {
                    _logger.LogWarning($"{logPrefix} - {_userId} already has an active membership.");
                    return RedirectToAction(nameof(ExistingMembership));
                }

                var membershipPlan = GetMembershipPlan(model.Membership.MembershipPlanId);

                if (membershipPlan == null)
                {
                    _logger.LogError($"{logPrefix} - Could not find membership plan with id {model.Membership.MembershipPlanId}");
                    return View("Error");
                }

                model.Membership.EndDate = DateTime.Today.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);

                if (membershipPlan.Type == (int)MembershipType.Monthly)
                {
                    model.Membership.EndDate = DateTime.Today.AddMonths((int)membershipPlan.NumberOfMonths).AddDays(-1).AddHours(23).AddMinutes(59);
                }

                model.Membership.StartDate = DateTime.Today;
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
        public async Task<IActionResult> MembershipDetails(int membershipId)
        {
            try
            {
                Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (affiliateId == 0 || string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId} or userId is missing");
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                var existingMembership = CheckForExistingMembership();

                if (existingMembership != null)
                {
                    _logger.LogWarning($"{logPrefix} - {_userId} already has an active membership.");
                    return RedirectToAction(nameof(ExistingMembership));
                }

                var membership = GetMembershipByMembershipIdAndMemberId(membershipId);

                if (membership == null)
                {
                    _logger.LogWarning($"{logPrefix} - Could not find membership with id {membershipId} for user with id: {_userId}");
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
                var existingMembership = CheckForExistingMembership();

                if (existingMembership != null)
                {
                    _logger.LogWarning($"{logPrefix} - {_userId} already has an active membership.");
                    return RedirectToAction(nameof(ExistingMembership));
                }

                var membership = GetMembershipByMembershipIdAndMemberId(model.MembershipId);

                if (membership == null)
                {
                    _logger.LogError($"{logPrefix} - Could not find membership with id {model.MembershipId} for user with id: {_userId}");
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
        public async Task<IActionResult> Review(int membershipId)
        {
            try
            {
                Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (affiliateId == 0 || string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId} or userId is missing");
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                var existingMembership = CheckForExistingMembership();

                if (existingMembership != null)
                {
                    _logger.LogWarning($"{logPrefix} - {_userId} already has an active membership.");
                    return RedirectToAction(nameof(ExistingMembership));
                }

                var membership = _repositoryWrapper.MembershipRepository
                    .FindByCondition(m => m.MembershipId == membershipId && m.MemberId == _userId)
                    .Include(m => m.MembershipPlan)
                    .FirstOrDefault();

                if (membership == null)
                {
                    _logger.LogError($"{logPrefix} - Could not find membership with id {membershipId} for user with id: {_userId}");
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
        public async Task<IActionResult> PaymentDetails(int membershipId)
        {
            try
            {
                Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (affiliateId == 0 || string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId} or userId is missing");
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                var existingMembership = CheckForExistingMembership();

                if (existingMembership != null)
                {
                    _logger.LogWarning($"{logPrefix} - {_userId} already has an active membership.");
                    return RedirectToAction(nameof(ExistingMembership));
                }

                var membership = GetMembershipByMembershipIdAndMemberId(membershipId);

                if (membership == null)
                {
                    _logger.LogError($"{logPrefix} - Could not find membership with id {membershipId} for user with id: {_userId}");
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
            if (string.IsNullOrEmpty(stripeToken) || membershipId == 0)
            {
                _logger.LogError($"{logPrefix} - {_userId} is trying to purchase membership but stripeToken = {stripeToken} and membershipId = {membershipId}");
                return View("Error");
            }

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
                    .Include(m => m.MembershipPlan)
                    .FirstOrDefault();

                if (membership == null)
                {
                    _logger.LogError($"{logPrefix} - Could not find membership with id {membershipId} for user with id: {_userId}");
                    return View("Error");
                }

                Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
                var affiliate = GetAffiliateById(affiliateId);

                if (affiliate == null)
                {
                    _logger.LogError($"{logPrefix} - Could not find affiliate with id {affiliateId}");
                    return View("Error");
                }

                if (string.IsNullOrEmpty(affiliate.ConnectedAccountId))
                {
                    _logger.LogError($"{logPrefix} - Affiliate with id {affiliateId} does not have ConnectedAccountId.");
                    return View("Error");
                }

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

                    string userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    var emailSubject = "Membership Purchase Confirmation";
                    var emailBody = $@"Hi {User.Identity.Name}, <br/><br/>
                    Thank you for purchasing a membership. Outlined below are the details of your membership: <br/><br/>
                    <div class='mb-3'>
                        <table style='border-collapse: collapse; width: 100%;'>
                            <tbody>
                                <tr style='border-bottom: 1px solid #ddd;'>
                                    <td style='padding: 8px;'><b>Membership:</b></td>
                                    <td style='padding: 8px;'>{membership.MembershipPlan.Title}</td>
                                </tr>
                                <tr style='border-bottom: 1px solid #ddd;'>
                                    <td style='padding: 8px;'><b>Price:</b></td>
                                    <td style='padding: 8px;'>{string.Format("{0:N2}", membership.MembershipPlan.Price)}</td>
                                </tr>";

                    if (membership.MembershipPlan.Type == (int)MembershipType.Weekly)
                    {
                        emailBody += $@"
                                    <tr style='border-bottom: 1px solid #ddd;'>
                                        <td style='padding: 8px;'><b>Number of Classes Per Week:</b></td>
                                        <td style='padding: 8px;'>{membership.MembershipPlan.NumberOfClasses}</td>
                                    </tr>";
                    }
                    else if (membership.MembershipPlan.Type == (int)MembershipType.Monthly)
                    {
                        emailBody += $@"
                                    <tr style='border-bottom: 1px solid #ddd;'>
                                        <td style='padding: 8px;'><b>Number of Classes:</b></td>
                                        <td style='padding: 8px;'>{membership.MembershipPlan.NumberOfClasses}</td>
                                    </tr>
                                    <tr style='border-bottom: 1px solid #ddd;'>
                                        <td style='padding: 8px;'><b>Number of Months:</b></td>
                                        <td style='padding: 8px;'>{membership.MembershipPlan.NumberOfMonths}</td>
                                    </tr>";
                    }
                    else if (membership.MembershipPlan.Type == (int)MembershipType.Unlimited)
                    {
                        emailBody += $@"
                                    <tr style='border-bottom: 1px solid #ddd;'>
                                        <td style='padding: 8px;'><b>Number of Classes:</b></td>
                                        <td style='padding: 8px;'>Unlimited</td>
                                    </tr>";
                    }

                    emailBody += $@"
                                <tr style='border-bottom: 1px solid #ddd;'>
                                    <td style='padding: 8px;'><b>Start Date:</b></td>
                                    <td style='padding: 8px;'>{membership.StartDate.ToShortDateString()}</td>
                                </tr>
                                <tr style='border-bottom: 1px solid #ddd;'>
                                    <td style='padding: 8px;'><b>End Date:</b></td>
                                    <td style='padding: 8px;'>{membership.EndDate?.ToShortDateString()}</td>
                                </tr>
                                <tr style='border-bottom: 1px solid #ddd;'>
                                    <td style='padding: 8px;'><b>Auto Renew:</b></td>
                                    <td style='padding: 8px;'>{(membership.AutoRenew ? "Yes" : "No")}</td>
                                </tr>
                                </tbody>
                                </table>
                                </div>";

                    await _emailSender.SendEmailAsync(userEmail, emailSubject, emailBody);

                    return RedirectToAction(NextStep.Name);
                }
                else
                {
                    _logger.LogError($"Payment failed for user with id {_userId} membership id {membershipId}. Stripe charge id: {processPayment.PaymentId}");
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
                .Where(mp => mp.AffiliateId == affiliateId && mp.IsActive)
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