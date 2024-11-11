using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrossPlanner.Repository.Wrapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;
using Microsoft.EntityFrameworkCore;
using CrossPlanner.Domain.Enums;
using CrossPlanner.Service.Stripe;
using System;

namespace CrossPlanner.Member.Controllers
{
    [Authorize(Roles = "Member")]
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStripeService _stripeService;
        private const string logPrefix = "Ctlr|Profile";
        private const int MaxFileSize = 1 * 1024 * 1024;
        private const string ProfilePictureFolderPath = "wwwroot/images/profile-pictures";

        public ProfileController(
            ILogger<ProfileController> logger,
            IRepositoryWrapper repositoryWrapper,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IStripeService stripeService)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _signInManager = signInManager;
            _userManager = userManager;
            _stripeService = stripeService;
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
                .FindByCondition(m =>
                m.MemberId == userId
                && m.IsActive
                && m.MembershipPlan.AffiliateId == affiliateId)
                .Include(m => m.MembershipPlan)
                .FirstOrDefault();

            var membershipId = 0;

            if (membership != null)
                membershipId = membership.MembershipId;

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
                MembershipAutoRenew = membership?.AutoRenew,
                MembershipStartDate = membership?.StartDate,
                MembershipEndDate = membership?.EndDate,
                MembershipId = membershipId,
            };

            return View(profileViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CancelMembership(int membershipId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to cancel membership with id {membershipId}");
            using var transaction = _repositoryWrapper.BeginTransaction();

            try
            {
                var membership = _repositoryWrapper.MembershipRepository
                    .FindByCondition(m => m.MembershipId == membershipId)
                    .Include(m => m.MembershipPlan)
                    .FirstOrDefault();

                if (membership == null || string.IsNullOrEmpty(membership.LastPaymentId))
                {
                    _logger.LogWarning($"{logPrefix} - Membership with id {membershipId} not found or has no payment record.");
                    return JsonResponse(false, "Membership could not be found.");
                }

                if (!TryGetAffiliate(out var affiliate, out string affiliateError))
                {
                    _logger.LogError($"{logPrefix} - {affiliateError}");
                    return JsonResponse(false, "Unable to cancel membership due to affiliate issue.");
                }

                decimal refundAmount = CalculateRefundAmount(membership);
                var refundResult = await ProcessRefund(membership, affiliate, refundAmount);

                if (!refundResult.Success)
                {
                    _logger.LogError($"{logPrefix} - Refund failed for membership with id {membershipId}");
                    return JsonResponse(false, "Unable to process refund.");
                }

                UpdateMembershipStatusToCancelled(membership);
                _repositoryWrapper.ScheduledClassReservationRepository.DeleteFutureScheduledClassReservations(membershipId, DateTime.Now);
                LogRefundTransaction(membership, refundAmount, refundResult.RefundId);

                _repositoryWrapper.Save();
                transaction.Commit();

                _logger.LogInformation($"{logPrefix} - Membership with id {membershipId} has been successfully canceled");
                return JsonResponse(true, "Membership canceled successfully.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError($"{logPrefix} - An error occurred while attempting to cancel membership with id {membershipId}: {ex}");
                return Json(new { message = "An error occurred while canceling membership." });
            }
            
        }

        private bool TryGetAffiliate(out Affiliate affiliate, out string error)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            affiliate = GetAffiliateById(affiliateId);

            if (affiliate == null)
            {
                error = $"Affiliate with id {affiliateId} not found.";
                return false;
            }

            if (string.IsNullOrEmpty(affiliate.ConnectedAccountId))
            {
                error = $"Affiliate with id {affiliateId} has no ConnectedAccountId.";
                return false;
            }

            error = null;
            return true;
        }

        private async Task<(bool Success, string RefundId)> ProcessRefund(Membership membership, Affiliate affiliate, decimal refundAmount)
        {
            var refundResult = await _stripeService.RefundCustomer(refundAmount, membership.LastPaymentId, affiliate.ConnectedAccountId);
            if (!refundResult.Success)
            {
                _logger.LogError($"{logPrefix} - Refund failed: {refundResult.Message}");
            }
            return (refundResult.Success, refundResult.RefundId);
        }

        private void UpdateMembershipStatusToCancelled(Membership membership)
        {
            membership.IsActive = false;
            membership.AutoRenew = false;
            membership.PaymentStatus = PaymentStatus.Refunded;
            _repositoryWrapper.MembershipRepository.Update(membership);
        }

        private void LogRefundTransaction(Membership membership, decimal refundAmount, string refundId)
        {
            var refund = new Refund
            {
                MembershipId = membership.MembershipId,
                Amount = refundAmount,
                RefundDate = DateTime.Now,
                StripeRefundId = refundId,
            };

            _repositoryWrapper.RefundRepository.Create(refund);
        }

        private decimal CalculateRefundAmount(Membership model)
        {
            DateTime endDate = model.EndDate ?? DateTime.Now;
            DateTime startDate = model.StartDate;
            decimal lastPaymentAmount = (decimal)model.LastPaymentAmount;
            
            if (model.MembershipPlan.Type == (int)MembershipType.Monthly)
            {
                int totalAllowedClasses = model.MembershipPlan.NumberOfClasses ?? 0;

                if (totalAllowedClasses == 0)
                {
                    _logger.LogWarning($"{logPrefix} - Total allowed classes for membership with id {model.MembershipId} is zero.");
                    return 0;
                }

                int attendedClasses = _repositoryWrapper.ScheduledClassReservationRepository
                    .FindByCondition(scr =>
                        scr.MembershipId == model.MembershipId &&
                        (scr.IsPresent == null || scr.IsPresent == true) &&
                        DateTime.Now < scr.ScheduledClass.StartDateTime &&
                        scr.ScheduledClass.IsActive &&
                        !scr.ScheduledClass.IsCancelled &&
                        !scr.ScheduledClass.IsDeleted)
                        .Count();

                decimal costPerClass = lastPaymentAmount / totalAllowedClasses;
                int unattendedClasses = Math.Max(0, totalAllowedClasses - attendedClasses);

                decimal refundAmount = costPerClass * unattendedClasses;
                _logger.LogInformation($"{logPrefix} - Calculated refund for {unattendedClasses} unused classes out of {totalAllowedClasses} for membership id {model.MembershipId}");

                return refundAmount;
            }
            else if (model.MembershipPlan.Type == (int)MembershipType.Weekly)
            {
                int totalWeeks = (endDate - startDate).Days / 7;
                int weeksUsed = (DateTime.Now - startDate).Days / 7;
                int weeksLeft = Math.Max(0, totalWeeks - weeksUsed);

                if (totalWeeks == 0)
                {
                    _logger.LogWarning($"{logPrefix} - Total weeks for membership with id {model.MembershipId} is zero.");
                    return 0;
                }

                decimal weeklyCost = lastPaymentAmount / totalWeeks;
                decimal refundAmount = weeklyCost * weeksLeft;
                _logger.LogInformation($"{logPrefix} - Calculated refund of {refundAmount} for {weeksLeft} unused weeks out of {totalWeeks} total weeks for membership id {model.MembershipId}");

                return refundAmount;
            }
            else if (model.MembershipPlan.Type == (int)MembershipType.Unlimited)
            {
                int totalDays = (endDate - startDate).Days;
                int usedDays = (DateTime.Now - startDate).Days;
                int unusedDays = Math.Max(0, totalDays - usedDays);

                if (totalDays == 0)
                {
                    _logger.LogWarning($"{logPrefix} - Total days for membership with id {model.MembershipId} is zero.");
                    return 0;
                }

                decimal dailyRate = lastPaymentAmount / totalDays;
                decimal refundAmount = dailyRate * unusedDays;
                _logger.LogInformation($"{logPrefix} - Calculated refund of {refundAmount} for {unusedDays} unused days out of {totalDays} total days for membership id {model.MembershipId}");

                return refundAmount;
            }
            else
            {
                _logger.LogWarning($"{logPrefix} - Encountered an unrecognized membership type for membership with id: {model.MembershipId}");
                return 0;
            }
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
                Email = user.Email,
                DisplayNameVisibility = user.DisplayNameVisibility,
                ProfilePictureVisibility = user.ProfilePictureVisibility
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
                user.DisplayNameVisibility = model.DisplayNameVisibility;
                user.ProfilePictureVisibility = model.ProfilePictureVisibility;
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
            if (!IsValidProfilePicture(profilePicture, out string validationError))
            {
                return JsonResponse(false, validationError);
            }

            var user = await _userManager.FindByIdAsync(model.ApplicationUserId);
            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - User with id {model.ApplicationUserId} not found.");
                return JsonResponse(false, "User not found.");
            }

            try
            {
                string fileName = $"{user.Id}{Path.GetExtension(profilePicture.FileName)}";
                string filePath = GetFilePath(fileName);

                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    string existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl.TrimStart('/'));

                    if (System.IO.File.Exists(existingFilePath))
                    {
                        try
                        {
                            System.IO.File.Delete(existingFilePath);
                            _logger.LogInformation($"{logPrefix} - Deleted existing profile picture at {existingFilePath} for user {model.ApplicationUserId}");
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogWarning(deleteEx, $"{logPrefix} - Failed to delete existing profile picture for user {model.ApplicationUserId}");
                        }
                    }
                }

                await SaveProfilePictureAsync(profilePicture, filePath);

                user.ProfilePictureUrl = $"/images/profile-pictures/{fileName}";
                var updateUserResult = await _userManager.UpdateAsync(user);

                if (!updateUserResult.Succeeded)
                {
                    _logger.LogError($"{logPrefix} - Error updating user profile picture for {model.ApplicationUserId}");
                    return JsonResponse(false, "Error updating profile picture. Please try again.");
                }

                _logger.LogInformation($"{logPrefix} - Profile picture updated successfully for user {model.ApplicationUserId}");
                return JsonResponse(true, "Profile picture updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{logPrefix} - Error uploading profile picture for user {model.ApplicationUserId}");
                return JsonResponse(false, "An error occurred while uploading the profile picture. Please try again.");
            }
        }

        private bool IsValidProfilePicture(IFormFile file, out string errorMessage)
        {
            errorMessage = null;
            if (file == null || file.Length == 0)
            {
                errorMessage = "Please upload a valid image.";
                return false;
            }

            if (file.Length > MaxFileSize)
            {
                errorMessage = "The uploaded image is too large. Please upload an image smaller than 1MB.";
                return false;
            }

            return true;
        }

        private string GetFilePath(string fileName)
        {
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), ProfilePictureFolderPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return Path.Combine(directoryPath, fileName);
        }

        private async Task SaveProfilePictureAsync(IFormFile profilePicture, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(stream);
            }
        }

        private JsonResult JsonResponse(bool success, string message)
        {
            return Json(new { success, message });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProfilePicture(string applicationUserId)
        {
            var user = await _userManager.FindByIdAsync(applicationUserId);
            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to delete profile picture for user with id {applicationUserId} was not found");
                return NotFound();
            }

            try
            {
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    string existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl.TrimStart('/'));

                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                        _logger.LogInformation($"{logPrefix} - Profile picture file deleted from path {existingFilePath}");
                    }

                    user.ProfilePictureUrl = null;
                    var updateUserResult = await _userManager.UpdateAsync(user);

                    if (!updateUserResult.Succeeded)
                    {
                        string errors = string.Join("; ", updateUserResult.Errors.Select(e => e.Description));
                        _logger.LogWarning($"{logPrefix} - Failed to update user after deleting profile picture for user with id {applicationUserId}: {errors}");
                    }
                    else
                    {
                        _logger.LogInformation($"{logPrefix} - Successfully deleted profile picture for user with id {applicationUserId}");
                    }
                }
                else
                {
                    _logger.LogInformation($"{logPrefix} - No profile picture found for user with id {applicationUserId}.");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"{logPrefix} - An error occurred deleting profile picture for user with id {applicationUserId}: {ex}");
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Bills()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bills = _repositoryWrapper.MembershipRepository
                .FindByCondition(m => m.MemberId == userId &&
                !string.IsNullOrEmpty(m.LastPaymentId) &&
                m.PaymentStatus != null &&
                m.LastPaymentAmount != null)
                .Include(m => m.MembershipPlan)
                .Include(m => m.MembershipPlan.Affiliate)
                .Include(m => m.Member)
                .Include(m => m.Refunds)
                .ToList();

            return View(bills);
        }

        [HttpGet]
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

        private Affiliate GetAffiliateById(int affiliateId)
        {
            return _repositoryWrapper.AffiliateRepository
                .FindByCondition(a => a.AffiliateId == affiliateId)
                .FirstOrDefault();
        }
    }
}