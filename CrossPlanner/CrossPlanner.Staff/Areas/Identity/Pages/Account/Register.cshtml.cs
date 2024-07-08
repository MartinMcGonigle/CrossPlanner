using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace CrossPlanner.Staff.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IRepositoryWrapper repositoryWrapper,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _repositoryWrapper = repositoryWrapper;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [MaxLength(250)]
            [Display(Name = "Affiliate Name")]
            public string Name { get; set; }

            [Required]
            [MaxLength(500)]
            [Display(Name = "Affiliate Address")]
            public string AffiliateAddress { get; set; }

            [Required]
            [MaxLength(100)]
            [Display(Name = "Affiliate Phone Number")]
            public string Phone { get; set; }

            [Required]
            [MaxLength(100)]
            [Display(Name = "Affiliate Email")]
            [EmailAddress]
            public string AffiliateEmail { get; set; }

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
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }


            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            using var transaction = _repositoryWrapper.BeginTransaction();

            try
            {
                var affiliate = CreateAffiliate(Input);

                _repositoryWrapper.AffiliateRepository.Create(affiliate);
                await _repositoryWrapper.SaveAsync();

                var user = CreateApplicationUser(Input);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    var affiliateUser = CreateAffiliateUser(affiliate.AffiliateId, user.Id);
                    
                    _repositoryWrapper.AffiliateUsersRepository.Create(affiliateUser);
                    await _repositoryWrapper.SaveAsync();
                    await transaction.CommitAsync();
                    
                    _logger.LogInformation("Affiliate, ApplicationUser, and AffiliateUser created successfully.");

                    await EnsureRoleExists("Owner");

                    var userRoleResult = await _userManager.AddToRoleAsync(user, "Owner");
                    if (userRoleResult.Succeeded)
                        _logger.LogInformation("User role added.");
                    else
                        _logger.LogError($"Unable to add user {user} to the role 'Owner'");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl, email =  Input.Email },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
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
                _logger.LogError($"An error occurred creating Affiliate, ApplicationUser, and AffiliateUser: {ex}");

                if (ex.InnerException.Message.Contains(Input.AffiliateEmail, StringComparison.CurrentCultureIgnoreCase))
                {
                    ModelState.AddModelError("", $"Affiliate Email '{Input.AffiliateEmail}' is already taken");
                }
                else
                {
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
            }

            return Page();
        }

        private Affiliate CreateAffiliate(InputModel model)
        {
            var affiliate = new Affiliate
            {
                Name = model.Name,
                Address = model.AffiliateAddress,
                Phone = model.Phone,
                Email = model.AffiliateEmail,
                IsActive = true,
            };

            return affiliate;
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

        private async Task EnsureRoleExists(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    _logger.LogError($"Error creating role {roleName}: {roleResult.Errors.FirstOrDefault()?.Description}");
                    throw new InvalidOperationException($"Failed to create role {roleName}");
                }
            }
        }
    }
}