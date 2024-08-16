using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using CrossPlanner.Domain.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using CrossPlanner.Domain.OtherModels;

namespace CrossPlanner.Staff.Controllers
{
    [Authorize(Roles = "SuperUser,Manager")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private const string logPrefix = "Ctlr|User";

        public UserController(
            ILogger<UserController> logger,
            IRepositoryWrapper repositoryWrapper,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index(string q, int page = 1, int pageSize = 25, string linkedToGymSearch = "0", string emailConfirmedSearch = "0", string activeMembershipSearch = "0", string roleSearch = "0")
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            _logger.LogInformation($"{logPrefix} - Attempting to display all users of affiliate with id {affiliateId}");

            var data = _repositoryWrapper.ApplicationUserRepository.GetAffiliateUsers(q, affiliateId, page, pageSize, linkedToGymSearch, emailConfirmedSearch, activeMembershipSearch, roleSearch);
            var count = _repositoryWrapper.ApplicationUserRepository.GetAffiliateUsersCount(q, affiliateId, linkedToGymSearch, emailConfirmedSearch, activeMembershipSearch, roleSearch);

            ViewData["CurrentFilter"] = q;
            ViewData["LinkedToGymSearch"] = linkedToGymSearch;
            ViewData["EmailConfirmedSearch"] = emailConfirmedSearch;
            ViewData["ActiveMembershipSearch"] = activeMembershipSearch;
            ViewData["RoleSearch"] = roleSearch;

            // Paging
            ViewData["Page"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["RecordCount"] = count;
            ViewData["Action"] = "Index";

            return View(data);
        }

        [HttpPut]
        public async Task<IActionResult> DeactivateAccount(string userId)
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
                user.PasswordHash = null;
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

        [HttpGet]
        public async Task<IActionResult> GetDeactivatedAccounts()
        {
            try
            {
                _logger.LogInformation($"{logPrefix} - Attempting to retrieve deactivated accounts");
                var deactivatedUsers = await _repositoryWrapper.ApplicationUserRepository.GetDeactivatedUsers();

                return Ok(deactivatedUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - Unable to retrieve deactivated accounts: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ReviewUserDetails(string userId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to retrieve details for user with id {userId}");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to retrieve details as user with id {userId} was not found.");
                return RedirectToAction("Index");
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
                return RedirectToAction("Index");
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

                        return RedirectToAction("Index");
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