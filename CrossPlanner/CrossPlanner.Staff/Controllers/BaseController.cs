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
    }
}