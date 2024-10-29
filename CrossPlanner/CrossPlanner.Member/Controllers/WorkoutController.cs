using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Identity;
using CrossPlanner.Domain.Models;
using Microsoft.EntityFrameworkCore;
using CrossPlanner.Domain.OtherModels;
using System.Security.Claims;

namespace CrossPlanner.Member.Controllers
{
    [Authorize(Roles = "Member")]
    public class WorkoutController : Controller
    {
        private readonly ILogger<WorkoutController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private const string logPrefix = "Ctlr|Workout";

        public WorkoutController(
            ILogger<WorkoutController> logger,
            IRepositoryWrapper repositoryWrapper,
            SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index(DateTime? date)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (affiliateId == 0 || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId} or userId is missing");
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var selectedDate = date ?? DateTime.Today;
            _logger.LogInformation($"{logPrefix} - Attempting to display workouts for affiliate with id {affiliateId} on {selectedDate.ToShortDateString()}");

            var workouts = _repositoryWrapper.WorkoutRepository
                .FindByCondition(w =>
                w.ClassType.AffiliateId == affiliateId
                && w.Date == selectedDate
                && w.IsActive
                && !w.IsDeleted)
                .Include(w => w.ClassType)
                .ToList();

            var viewModel = new DailyWorkoutViewModel
            {
                SelectedDate = selectedDate,
                Workouts = workouts
            };

            return View(viewModel);
        }
    }
}