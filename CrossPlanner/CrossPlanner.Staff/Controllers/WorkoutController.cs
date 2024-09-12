using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Identity;
using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;
using Microsoft.EntityFrameworkCore;

namespace CrossPlanner.Staff.Controllers
{
    [Authorize(Roles = "SuperUser,Manager")]
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

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            var selectedDate = date ?? DateTime.Today;

            if (affiliateId == 0)
            {
                _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId}");
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            
            _logger.LogInformation($"{logPrefix} - Attempting to display workouts for affiliate with id {affiliateId} on {selectedDate.ToShortDateString()}");

            var workouts = _repositoryWrapper.WorkoutRepository
                .FindByCondition(w => w.ClassType.AffiliateId == affiliateId
                && w.Date == selectedDate
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

            var classTypes = _repositoryWrapper.ClassTypeRepository.GetAffiliateActiveClassTypes(affiliateId);

            var viewModel = new WorkoutViewModel
            {
                ClassTypes = classTypes,
                Workout = new Workout()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkoutViewModel model)
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
                if (!ModelState.IsValid)
                {
                    model.ClassTypes = _repositoryWrapper.ClassTypeRepository.GetAffiliateActiveClassTypes(affiliateId);
                    return View(model);
                }

                _logger.LogInformation($"{logPrefix} - Attempting to create workout for affiliate with id {affiliateId}");
                _repositoryWrapper.WorkoutRepository.Create(model.Workout);
                await _repositoryWrapper.SaveAsync();

                TempData["SuccessMessage"] = "Workout created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - Error creating workout: {ex}");
                ModelState.AddModelError("", "Error creating workout. Please try again or contact support.");
                model.ClassTypes = _repositoryWrapper.ClassTypeRepository.GetAffiliateActiveClassTypes(affiliateId);

                return View(model);
            }
        }

        [HttpPut]
        public async Task<IActionResult> ToggleWorkout(int workoutId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to toggle workout with id {workoutId}");

            var workout = GetWorkoutById(workoutId);
            if (workout == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to toggle workout with id {workoutId} as could not be found.");
                return Json(new { message = "Workout not found." });
            }

            try
            {
                workout.IsActive = !workout.IsActive;
                _repositoryWrapper.WorkoutRepository.Update(workout);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Workout toggled successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to toggle workout with id {workoutId}: {ex}");
                return Json(new { message = "An error occurred while toggling workout." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteWorkout(int workoutId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to delete workout with id {workoutId}");

            var workout = GetWorkoutById(workoutId);
            if (workout == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to delete workout with id {workoutId} as could not be found");
                return Json(new { message = "Workout not found." });
            }

            try
            {
                workout.IsActive = false;
                workout.IsDeleted = true;

                _repositoryWrapper.WorkoutRepository.Update(workout);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Workout deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to delete workout with id {workoutId}: {ex}");
                return Json(new { message = "An error occurred while deleting workout." });
            }
        }

        [HttpGet]
        public IActionResult Edit(int workoutId)
        {
            var workout = GetWorkoutById(workoutId);

            if (workout == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit workout with id {workoutId} was not found");
                TempData["ErrorMessage"] = "Workout not found.";
                return RedirectToAction("Index");
            }

            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);

            var classTypes = _repositoryWrapper.ClassTypeRepository.GetAffiliateActiveClassTypes(affiliateId);

            var viewModel = new WorkoutViewModel
            {
                ClassTypes = classTypes,
                Workout = workout
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(WorkoutViewModel viewModel)
        {
            var workout = GetWorkoutById(viewModel.Workout.WorkoutId);

            if (workout == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit workout with id {viewModel.Workout.WorkoutId} was not found.");
                TempData["ErrorMessage"] = "Workout not found.";
                return RedirectToAction("Index");
            }

            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            try
            {
                if (!ModelState.IsValid)
                {
                    viewModel.ClassTypes = _repositoryWrapper.ClassTypeRepository.GetAffiliateActiveClassTypes(affiliateId).ToList();
                    return View(viewModel);
                }

                _logger.LogInformation($"{logPrefix} - Attempting to edit workout with id {viewModel.Workout.WorkoutId}");

                workout.Description = viewModel.Workout.Description;
                workout.ClassTypeId = viewModel.Workout.ClassTypeId;
                workout.Date = viewModel.Workout.Date;

                _repositoryWrapper.WorkoutRepository.Update(workout);
                _repositoryWrapper.Save();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - Error editing workout: {ex}");
                ModelState.AddModelError("", "Error editing workout. Please try again or contact support.");

                viewModel.ClassTypes = _repositoryWrapper.ClassTypeRepository.GetAffiliateActiveClassTypes(affiliateId);

                return View(viewModel);
            }
        }

        private Workout? GetWorkoutById(int workoutId)
        {
            return _repositoryWrapper.WorkoutRepository
                .FindByCondition(w => w.WorkoutId == workoutId)
                .FirstOrDefault();
        }
    }
}