using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrossPlanner.Repository.Wrapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;
using Microsoft.EntityFrameworkCore;

namespace CrossPlanner.Member.Controllers
{
    [Authorize(Roles = "Member")]
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string logPrefix = "Ctlr|Profile";

        public ProfileController(
            ILogger<ProfileController> logger,
            IRepositoryWrapper repositoryWrapper,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _signInManager = signInManager;
            _userManager = userManager;
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

            var applicationUser = _repositoryWrapper.ApplicationUserRepository
                .FindByCondition(au => au.Id == userId)
                .FirstOrDefault();

            if (applicationUser == null)
            {
                _logger.LogWarning($"{logPrefix} - Member with id {userId} not found.");
                return NotFound();
            }

            var membership = _repositoryWrapper.MembershipRepository
                .FindByCondition(m => m.MemberId == userId
                && m.IsActive
                && m.MembershipPlan.AffiliateId == affiliateId)
                .Include(m => m.MembershipPlan)
                .FirstOrDefault();

            var profileViewModel = new ProfileViewModel
            {
                ApplicationUserId = applicationUser.Id,
                FirstName = applicationUser.FirstName,
                LastName = applicationUser.LastName,
                Email = applicationUser.Email,
                DateOfBirth = applicationUser.DateOfBirth,
                ProfilePictureUrl = applicationUser.ProfilePictureUrl,
                DisplayNameVisibility = applicationUser.DisplayNameVisibility,
                ProfilePictureVisibility = applicationUser.ProfilePictureVisibility,
                MembershipPlanTitle = membership?.MembershipPlan?.Title,
                MembershipAutoRenew = (bool)(membership?.AutoRenew),
                MembershipStartDate = (DateTime)(membership?.StartDate),
                MembershipEndDate = membership?.EndDate,
            };

            return View(profileViewModel);
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(string applicationUserId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to edit user with id {applicationUserId}");

            var user = await _userManager.FindByIdAsync(applicationUserId);
            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit user with id {applicationUserId} was not found");
                return NotFound();
            }

            var viewModel = new EditAccountViewModel
            {
                ApplicationUserId = applicationUserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.ApplicationUserId);
            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit user with id {model.ApplicationUserId} was not found");
                return NotFound();
            }

            try
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.DateOfBirth = model.DateOfBirth;

                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    AddErrors(updateUserResult);
                    return View(model);
                }

                _logger.LogInformation($"{logPrefix} - Successfully updated user with id {model.ApplicationUserId}");
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred updating user with id {model.ApplicationUserId}: {ex}");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UploadProfilePicture(string applicationUserId)
        {
            var user = await _userManager.FindByIdAsync(applicationUserId);
            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - User with id {applicationUserId} not found.");
                return NotFound();
            }

            var viewModel = new UploadProfilePictureViewModel
            {
                ApplicationUserId = applicationUserId,
                CurrentProfilePictureUrl = user.ProfilePictureUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(UploadProfilePictureViewModel model, IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid image.");
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.ApplicationUserId);
            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - User with id {model.ApplicationUserId} not found.");
                return NotFound();
            }

            try
            {
                var fileExtension = Path.GetExtension(profilePicture.FileName);
                var fileName = $"{user.Id}{fileExtension}";
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile-pictures");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(directoryPath));
                }

                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                user.ProfilePictureUrl = $"/images/profile-pictures/{fileName}";
                var updateUserResult = await _userManager.UpdateAsync(user);

                if (!updateUserResult.Succeeded)
                {
                    AddErrors(updateUserResult);
                    return View(model);
                }

                _logger.LogInformation($"{logPrefix} - Profile picture updated successfully for user {model.ApplicationUserId}");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - Error uploading profile picture for user {model.ApplicationUserId}: {ex}");
                ModelState.AddModelError("", "An error occurred while uploading the profile picture. Please try again.");
                return View(model);
            }
        }

        public IActionResult Bills()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bills = _repositoryWrapper.MembershipRepository
                .FindByCondition(m => m.MemberId == userId)
                .Include(m => m.MembershipPlan)
                .Include(m => m.MembershipPlan.Affiliate)
                .Include(m => m.Member)
                .Include(m => m.Refunds)
                .ToList();

            return View(bills);
        }

        public IActionResult AttendanceHistory()
        {
            var dateTime = DateTime.Now;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var scheduledClassReservations = _repositoryWrapper.ScheduledClassReservationRepository
                .FindByCondition(scr =>
                scr.Membership.MemberId == userId &&
                (scr.IsPresent == null || scr.IsPresent == true) &&
                dateTime > scr.ScheduledClass.StartDateTime &&
                scr.ScheduledClass.IsActive == true &&
                scr.ScheduledClass.IsCancelled != true &&
                scr.ScheduledClass.IsDeleted != true)
                .Include(scr => scr.ScheduledClass.ClassType.Affiliate)
                .Include(scr => scr.ScheduledClass.Instructor)
                .ToList();

            var startOfWeek = dateTime.AddDays(-(int)dateTime.DayOfWeek);
            var startOfMonth = new DateTime(dateTime.Year, dateTime.Month, 1);

            var attendanceThisWeek = scheduledClassReservations
                .Count(scr => scr.ScheduledClass.StartDateTime >= startOfWeek);

            var attendanceThisMonth = scheduledClassReservations
                .Count(scr => scr.ScheduledClass.StartDateTime >= startOfMonth);

            var viewModel = new AttendanceHistoryViewModel
            {
                AttendanceHistoryWeekly = attendanceThisWeek,
                AttendanceHistoryMonthly = attendanceThisMonth,
                ScheduledClassReservations = scheduledClassReservations
            };

            return View(viewModel);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}