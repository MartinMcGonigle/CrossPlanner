using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrossPlanner.Repository.Wrapper;
using CrossPlanner.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CrossPlanner.Member.Controllers
{
    [Authorize(Roles = "Member")]
    public class NotificationController : Controller
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private const string logPrefix = "Ctlr|Notification";

        public NotificationController(
            ILogger<NotificationController> logger,
            IRepositoryWrapper repositoryWrapper,
            SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (affiliateId == 0 || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId} or userId is missing");
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            _logger.LogInformation($"{logPrefix} - Attempting to display notifications for affiliate with id {affiliateId} and user id {userId}");

            var notifications = _repositoryWrapper.NotificationRepository
                .FindByCondition(n => n.AffiliateId == affiliateId
                && !n.IsDeleted
                && n.IsActive
                && n.UserAccess.Contains(userId))
                .OrderByDescending(n => n.NotificationId)
                .ToList();

            return View(notifications);
        }
    }
}
