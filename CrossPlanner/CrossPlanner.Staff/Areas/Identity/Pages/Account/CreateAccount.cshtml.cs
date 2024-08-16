using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace CrossPlanner.Staff.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "SuperUser,Manager")]
    public class CreateAccountModel : PageModel
    {
        private readonly ILogger<CreateAccountModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public CreateAccountModel(
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

        public string ReturnController { get; set; }

        public class InputModel
        {
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

        public async Task OnGetAsync(string returnUrl = null, string returnController = null)
        {
            ReturnUrl = returnUrl;
            ReturnController = returnController;

            var roles = _roleManager.Roles.ToList();
            ViewData["roles"] = roles;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null, string returnController = null)
        {
            returnUrl ??= Url.Content("~/");
            ReturnController = returnController;

            if (ModelState.IsValid)
            {
                using var transaction = _repositoryWrapper.BeginTransaction();

                try
                {
                    var user = CreateApplicationUser(Input);
                    var result = await _userManager.CreateAsync(user);

                    if (result.Succeeded)
                    {
                        Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
                        var affiliateUser = CreateAffiliateUser(affiliateId, user.Id);

                        _repositoryWrapper.AffiliateUsersRepository.Create(affiliateUser);
                        await _repositoryWrapper.SaveAsync();

                        var userRoleResult = await _userManager.AddToRoleAsync(user, Input.Role);

                        if (userRoleResult.Succeeded)
                        {
                            await transaction.CommitAsync();
                            _logger.LogInformation($"ApplicationUser, AspNetUserRoles, and AffiliateUser created successfully.");

                            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                            var callbackUrl = Url.Page(
                                "/Account/ConfirmAccount",
                                pageHandler: null,
                                values: new { area = "Identity", userId = user.Id, code = code },
                                protocol: Request.Scheme);

                            if (Input.Role == "Member")
                            {
                                var staff = _configuration.GetSection("WebUrls").GetSection("Staff").Value;
                                var member = _configuration.GetSection("WebUrls").GetSection("Member").Value;

                                callbackUrl = callbackUrl.Replace(staff, member);
                            }

                            await _emailSender.SendEmailAsync(Input.Email, "Confirm your account",
                                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                            return RedirectToAction("Index", ReturnController);
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
                    else
                    {
                        // If user email already exists in AspNetUsers this gets executed
                        transaction.Rollback();
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError($"An error occurred creating ApplicationUser, AspNetUserRoles, and AffiliateUser: {ex}");
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
            }

            var roles = _roleManager.Roles.ToList();
            ViewData["roles"] = roles;

            return Page();
        }

        private ApplicationUser CreateApplicationUser(InputModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                IsActive = true,
            };

            return user;
        }

        private AffiliateUser CreateAffiliateUser(int affiliateId, string applicationUserId)
        {
            var affiliateUser = new AffiliateUser
            {
                AffiliateId = affiliateId,
                ApplicationUserId = applicationUserId,
                IsActive = true,
            };

            return affiliateUser;
        }
    }
}