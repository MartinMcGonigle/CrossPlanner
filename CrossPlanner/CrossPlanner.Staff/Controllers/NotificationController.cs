using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Identity;
using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;
using System.Security.Claims;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace CrossPlanner.Staff.Controllers
{
    [Authorize(Roles = "SuperUser,Manager")]
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

            if (affiliateId == 0)
            {
                _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId}");
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var notifications = _repositoryWrapper.NotificationRepository
                .FindAll()
                .Where(n => n.AffiliateId == affiliateId && !n.IsDeleted)
                .OrderByDescending(n => n.NotificationId);

            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);

            if (affiliateId == 0)
            {
                _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId}");
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var affiliateUsers = _repositoryWrapper.AffiliateUsersRepository.GetAffiliateActiveUsers(affiliateId);

            var viewModel = new NotificationViewModel
            {
                AffiliateUsers = affiliateUsers,
                Notification = new Notification()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(NotificationViewModel viewModel)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);

            if (affiliateId == 0)
            {
                _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId}");
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            try
            {
                viewModel.Notification.UserCreated = User.FindFirstValue(ClaimTypes.NameIdentifier);
                viewModel.Notification.CreatedDate = DateTime.Now;
                viewModel.Notification.AffiliateId = affiliateId;

                if (viewModel.Notification.UserGrantAcess != null && viewModel.Notification.UserGrantAcess.Any())
                {
                    viewModel.Notification.UserAccess = string.Join(",", viewModel.Notification.UserGrantAcess);
                }
                ModelState.Clear();
                if (ModelState.IsValid)
                {
                    _repositoryWrapper.NotificationRepository.Create(viewModel.Notification);
                    await _repositoryWrapper.SaveAsync();

                    return RedirectToAction("Index");
                }

                viewModel.AffiliateUsers = _repositoryWrapper.AffiliateUsersRepository.GetAffiliateActiveUsers(affiliateId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - Error creating notification: {ex}");
                ModelState.AddModelError("", "Error creating notification. Please try again or contact support.");
                viewModel.AffiliateUsers = _repositoryWrapper.AffiliateUsersRepository.GetAffiliateActiveUsers(affiliateId);
                return View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult Edit(int notificationId)
        {
            var notification = GetNotificationById(notificationId);

            if (notification == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit notification with id {notificationId} was not found");
                TempData["ErrorMessage"] = "Notification not found.";
                return RedirectToAction("Index");
            }

            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);

            var affiliateUsers = _repositoryWrapper.AffiliateUsersRepository.GetAffiliateActiveUsers(affiliateId);
            notification.UserGrantAcess = notification.UserAccess != null ? notification.UserAccess.Split(',').ToList() : new List<string>();

            var viewModel = new NotificationViewModel
            {
                AffiliateUsers = affiliateUsers,
                Notification = notification
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(NotificationViewModel viewModel)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);

            if (affiliateId == 0)
            {
                _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId}");
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var notification = GetNotificationById(viewModel.Notification.NotificationId);

            if (notification == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit notification with id {viewModel.Notification.NotificationId} was not found.");
                TempData["ErrorMessage"] = "Notification not found.";
                return RedirectToAction("Index");
            }

            try
            {
                if (viewModel.Notification.UserGrantAcess != null && viewModel.Notification.UserGrantAcess.Any())
                {
                    viewModel.Notification.UserAccess = string.Join(",", viewModel.Notification.UserGrantAcess);
                }

                ModelState.Clear();

                if (ModelState.IsValid)
                {
                    _logger.LogInformation($"{logPrefix} - Attempting to edit notification with id {viewModel.Notification.NotificationId}");

                    notification.Title = viewModel.Notification.Title;
                    notification.Message = viewModel.Notification.Message;
                    notification.UserAccess = viewModel.Notification.UserAccess;

                    _repositoryWrapper.NotificationRepository.Update(notification);
                    await _repositoryWrapper.SaveAsync();

                    return RedirectToAction("Index");
                }

                viewModel.AffiliateUsers = _repositoryWrapper.AffiliateUsersRepository.GetAffiliateActiveUsers(affiliateId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - Error editing notification: {ex}");
                ModelState.AddModelError("", "Error editing notification. Please try again or contact support.");
                viewModel.AffiliateUsers = _repositoryWrapper.AffiliateUsersRepository.GetAffiliateActiveUsers(affiliateId);
                return View(viewModel);
            }
        }

        [HttpPut]
        public async Task<IActionResult> ToggleNotification(int notificationId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to toggle notification with id {notificationId}");

            var notification = GetNotificationById(notificationId);
            if (notification == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to toggle notification with id {notificationId} as could not be found.");
                return Json(new { message = "Notification not found." });
            }

            try
            {
                notification.IsActive = !notification.IsActive;
                _repositoryWrapper.NotificationRepository.Update(notification);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Notification toggled successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to toggle notification with id {notificationId}: {ex}");
                return Json(new { message = "An error occurred while toggling notification." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to delete notification with id {notificationId}");

            var notification = GetNotificationById(notificationId);
            if (notification == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to delete notification with id {notificationId} as could not be found.");
                return Json(new { message = "Notification not found." });
            }

            try
            {
                notification.IsActive = false;
                notification.IsDeleted = true;

                _repositoryWrapper.NotificationRepository.Update(notification);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Notification deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to delete notification with id {notificationId}: {ex}");
                return Json(new { message = "An error occurred while deleting notification." });
            }
        }

        private Notification? GetNotificationById(int notificationId)
        {
            return _repositoryWrapper.NotificationRepository
                .FindByCondition(n => n.NotificationId == notificationId)
                .FirstOrDefault();
        }
    }
}