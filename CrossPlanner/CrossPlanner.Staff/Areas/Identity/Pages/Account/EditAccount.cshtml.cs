using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CrossPlanner.Staff.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "SuperUser")]
    public class EditAccountModel : PageModel
    {
        private readonly ILogger<CreateAccountModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private const string logPrefix = "Page|EditAccount";

        public EditAccountModel(
            ILogger<CreateAccountModel> logger,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IRepositoryWrapper repositoryWrapper,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IEmailSender emailSender)
        {
            _logger = logger;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _repositoryWrapper = repositoryWrapper;
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public string ReturnController {  get; set; }

        public class InputModel
        {
            public string ApplicationUserId { get; set; }

            [MaxLength(100)]
            [Display(Name = "First Name")]
            [Required]
            public string FirstName { get; set; }

            [MaxLength(100)]
            [Display(Name = "Last Name")]
            [Required]
            public string LastName { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Date Of Birth")]
            public DateTime DateOfBirth { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "Role")]
            public string Role { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string applicationUserId, string returnUrl = null, string returnController = null)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to edit user with id {applicationUserId}");

            ReturnUrl = returnUrl;
            ReturnController = returnController;

            var user = await _userManager.FindByIdAsync(applicationUserId);
            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit user with id {applicationUserId} was not found.");
                return Redirect($"/{returnController}/Index");
            }

            Input = new InputModel
            {
                ApplicationUserId = applicationUserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
            };

            PopulateRoles();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null, string returnController = null)
        {
            returnUrl ??= Url.Content("~/");
            ReturnController = returnController;

            if (!ModelState.IsValid)
            {
                PopulateRoles();
                return Page();
            }

            var user = await _userManager.FindByIdAsync(Input.ApplicationUserId);
            if (user == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit user with id {Input.ApplicationUserId} was not found.");
                return Redirect($"/{returnController}/Index");
            }

            using var transaction = _repositoryWrapper.BeginTransaction();

            try
            {
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.DateOfBirth = Input.DateOfBirth;
                user.Email = Input.Email;
                user.UserName = Input.Email;

                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    transaction.Rollback();
                    AddErrors(updateUserResult);
                    PopulateRoles();
                    return Page();
                }

                if (await UpdateUserRole(user, Input.Role) == false)
                {
                    transaction.Rollback();
                    PopulateRoles();
                    return Page();
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"{logPrefix} - Successfully updated user with id {Input.ApplicationUserId}");
                return Redirect($"/{returnController}/Index");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError($"{logPrefix} - An error occurred updating user with id {Input.ApplicationUserId}: {ex}");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                PopulateRoles();
                return Page();
            }
        }

        private async Task<bool> UpdateUserRole(ApplicationUser user, string newRole)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(Input.Role))
            {
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeRolesResult.Succeeded)
                {
                    AddErrors(removeRolesResult);
                    return false;
                }

                var addRoleResult = await _userManager.AddToRoleAsync(user, newRole);
                if (!addRoleResult.Succeeded)
                {
                    AddErrors(addRoleResult);
                    return false;
                }
            }

            return true;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private void PopulateRoles()
        {
            var roles = _roleManager.Roles.ToList();
            ViewData["roles"] = roles;
        }
    }
}