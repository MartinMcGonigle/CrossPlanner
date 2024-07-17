using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Identity.UI.Services;
using CrossPlanner.Domain.OtherModels;
using System.Data;

namespace CrossPlanner.Staff.Controllers
{
    public class BaseController : Controller
    {
        internal readonly UserManager<ApplicationUser> _userManager;
        internal readonly IRepositoryWrapper _repositoryWrapper;
        internal readonly IEmailSender _emailSender;
        internal readonly ILogger<BaseController> _logger;
        internal readonly IConfiguration _configuration;
        internal readonly RoleManager<IdentityRole> _roleManager;
        protected const string logPrefix = "BaseCtrl";

        public BaseController(
            UserManager<ApplicationUser> userManager,
            IRepositoryWrapper repositoryWrapper,
            IEmailSender emailSender,
            ILogger<BaseController> logger,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _repositoryWrapper = repositoryWrapper;
            _emailSender = emailSender;
            _logger = logger;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> ResendVerification(string userId, string role)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to resend verification email to user with id {userId}");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to resend verification email as user with id {userId} was not found.");
                return Json(new { message = "User not found." });
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation($"{logPrefix} - Skipped resending verification email as user with id {userId} has already confirmed their email.");
                return Json(new { message = "Email is already confirmed." });
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            if (role.Trim().ToLower().Contains("member"))
            {
                var staff = _configuration.GetSection("WebUrls").GetSection("Staff").Value;
                var member = _configuration.GetSection("WebUrls").GetSection("Member").Value;

                callbackUrl = callbackUrl.Replace(staff, member);
            }

            await _emailSender.SendEmailAsync(
                user.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            _logger.LogInformation($"{logPrefix} - Verification email resent successfully to user with id {userId}");

            return Json(new { message = "Verification email resent successfully." });
        }

        [HttpPut]
        public async Task<IActionResult> DeactivateAccount(string userId, string role)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to deactivate user account with id {userId}");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to deactivate user account with id {userId} as user could not be found.");
                return Json(new { message = "User not found." });
            }

            using var transaction = _repositoryWrapper.BeginTransaction();

            try
            {
                user.EmailConfirmed = false;
                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    transaction.Rollback();
                    _logger.LogError($"{logPrefix} - Error occurred while updating user with id {userId}: {updateUserResult.Errors.FirstOrDefault()?.Description}");
                    return Json(new { message = "Error updating user account." });
                }

                var roleRemovalResult = await RemoveUserRoles(user);
                if (!roleRemovalResult.Succeeded)
                {
                    transaction.Rollback();
                    _logger.LogError($"{logPrefix} - Error occurred while removing roles from user with id {userId}: {roleRemovalResult.Errors.FirstOrDefault()?.Description}");
                    return Json(new { message = "Failed to remove user roles." });
                }

                var affiliateUserResult = await DeactivateAffiliateUser(user.Id);
                if (!affiliateUserResult)
                {
                    transaction.Rollback();
                    _logger.LogError($"{logPrefix} - Error occurred while updating AffiliateUser record for user with id {userId}");
                    return Json(new { message = "Error updating user account." });
                }

                transaction.Commit();

                _logger.LogInformation($"{logPrefix} - User account with ID: {userId} has been successfully deactivated");
                return Json(new { message = "User account deactivated." });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to deactivate user account with id {userId}: {ex}");
                return Json(new { message = "An error occurred while deactivating user account." });
            }
        }

        private async Task<IdentityResult> RemoveUserRoles(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return await _userManager.RemoveFromRolesAsync(user, roles);
        }

        private async Task<bool> DeactivateAffiliateUser(string userId)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);

            var affiliateUser = _repositoryWrapper.AffiliateUsersRepository
                .FindByCondition(x => x.ApplicationUserId == userId
                && x.AffiliateId == affiliateId
                && x.IsActive)
                .FirstOrDefault();

            if (affiliateUser != null)
            {
                affiliateUser.IsActive = false;
                _repositoryWrapper.AffiliateUsersRepository.Update(affiliateUser);
                await _repositoryWrapper.SaveAsync();

                return true;
            }

            return false;
        }

        [HttpGet]
        public async Task<IActionResult> GetDeactivatedAccounts()
        {
            try
            {
                _logger.LogInformation($"{logPrefix} - Attempting to retrieve deactivated accounts");
                var deactivatedUsers = await _repositoryWrapper.ApplicationUserRepository.GetDeactivatedUsers();

                return Ok(deactivatedUsers);
            }
            catch(Exception ex)
            {
                _logger.LogError($"{logPrefix} - Unable to retrieve deactivated accounts: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ReviewUserDetails(string userId, string userType)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to retrieve details for user with id {userId}");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to retrieve details as user with id {userId} was not found.");
                return RedirectToAction("Index", userType);
            }

            var roles = _roleManager.Roles.ToList();
            var model = new ReactivateUserViewModel
            {
                ApplicationUserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                Roles = roles.Select(r => new System.Web.Mvc.SelectListItem { Text = r.Name, Value = r.Name }).ToList(),
                ReturnController = userType,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ReviewUserDetails(ReactivateUserViewModel model)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to reactivate user account with id {model.ApplicationUserId}");

            var user = await _userManager.FindByIdAsync(model.ApplicationUserId);

            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to reactivate user account with id {model.ApplicationUserId} was not found");
                return RedirectToAction("Index", model.ReturnController);
            }

            if (ModelState.IsValid)
            {
                using var transaction = _repositoryWrapper.BeginTransaction();

                try
                {
                    Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
                    var affiliateUser = CreateAffiliateUser(affiliateId, user.Id);

                    if (affiliateUser.AffiliateUserId == 0)
                    {
                        _repositoryWrapper.AffiliateUsersRepository.Create(affiliateUser);
                    }
                    else
                    {
                        _repositoryWrapper.AffiliateUsersRepository.Update(affiliateUser);
                    }
                    await _repositoryWrapper.SaveAsync();

                    var userRoleResult = await _userManager.AddToRoleAsync(user, model.Role);

                    if (userRoleResult.Succeeded)
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation($"{logPrefix} - AspNetUserRoles, and AffiliateUser created/updated successfully.");

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                        var callbackUrl = Url.Page(
                            "/Account/ConfirmAccount",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code },
                            protocol: Request.Scheme);

                        if (model.Role == "Member")
                        {
                            var staff = _configuration.GetSection("WebUrls").GetSection("Staff").Value;
                            var member = _configuration.GetSection("WebUrls").GetSection("Member").Value;

                            callbackUrl = callbackUrl.Replace(staff, member);
                        }

                        await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        return RedirectToAction("Index", model.ReturnController);
                    }
                    else
                    {
                        // If there was an error adding AspNetUser to AspNetUserRoles this gets executed
                        transaction.Rollback();
                    }

                    foreach (var error in userRoleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError($"{logPrefix} - AspNetUserRoles, and AffiliateUser could not be created/updated: {ex}");
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
            }

            var roles = _roleManager.Roles.ToList();
            model.Roles = roles.Select(r => new System.Web.Mvc.SelectListItem { Text = r.Name, Value = r.Name }).ToList();

            return View(model);
        }

        private AffiliateUser CreateAffiliateUser(int affiliateId, string applicationUserId)
        {
            var affiliateUser = _repositoryWrapper.AffiliateUsersRepository
                .FindByCondition(x => x.AffiliateId == affiliateId && x.ApplicationUserId == applicationUserId)
                .FirstOrDefault();

            if (affiliateUser == null)
            {
                return new AffiliateUser
                {
                    AffiliateId = affiliateId,
                    ApplicationUserId = applicationUserId,
                    IsActive = true,
                };
            }

            affiliateUser.IsActive = true;

            return affiliateUser;
        }
    }
}