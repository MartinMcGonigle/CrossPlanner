using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrossPlanner.Repository.Wrapper;
using Microsoft.AspNetCore.Identity;
using CrossPlanner.Domain.Models;
using CrossPlanner.Domain.OtherModels;

namespace CrossPlanner.Staff.Controllers
{
    [Authorize(Roles = "SuperUser,Manager")]
    public class ScheduledClassController : Controller
    {
        private readonly ILogger<ScheduledClassController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private const string logPrefix = "Ctlr|ScheduledClass";
        private readonly UserManager<ApplicationUser> _userManager;

        public ScheduledClassController(
            ILogger<ScheduledClassController> logger,
            IRepositoryWrapper repositoryWrapper,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index(DateTime? date)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            var selectedDate = date ?? DateTime.Today;

            _logger.LogInformation($"{logPrefix} - Attempting to display scheduled classes for affiliate with id {affiliateId} on {selectedDate.ToShortDateString()}");

            var scheduledClasses = _repositoryWrapper.ScheduledClassRepository.GetAffiliateScheduledClassByDate(affiliateId, selectedDate);

            var viewModel = new DailyScheduleViewModel
            {
                SelectedDate = selectedDate,
                ScheduledClasses = scheduledClasses
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);

            var instructors = _repositoryWrapper.ApplicationUserRepository.GetAffiliateActiveStaff(affiliateId);
            var classTypes = _repositoryWrapper.ClassTypeRepository.GetAffiliateActiveClassTypes(affiliateId);

            var scheduledClassViewModel = new ScheduledClassViewModel
            {
                Instructors = instructors.ToList(),
                ClassTypes = classTypes,
                ScheduledClass = new ScheduledClass()
            };

            return View(scheduledClassViewModel);
        }

        [HttpPost]
        public IActionResult Create(ScheduledClassViewModel viewModel)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);

            try
            {
                if (!ModelState.IsValid)
                {
                    viewModel.Instructors = _repositoryWrapper.ApplicationUserRepository.GetAffiliateActiveStaff(affiliateId).ToList();
                    viewModel.ClassTypes = _repositoryWrapper.ClassTypeRepository.GetAffiliateActiveClassTypes(affiliateId).ToList();

                    return View(viewModel);
                }

                _logger.LogInformation($"{logPrefix} - Attempting to create scheduled class for affiliate with id {affiliateId}");
                _repositoryWrapper.ScheduledClassRepository.Create(viewModel.ScheduledClass);
                _repositoryWrapper.Save();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - Error creating scheduled class: {ex}");
                ModelState.AddModelError("", "Error creating scheduled class. Please try again or contact support.");

                viewModel.Instructors = _repositoryWrapper.ApplicationUserRepository.GetAffiliateActiveStaff(affiliateId).ToList();
                viewModel.ClassTypes = _repositoryWrapper.ClassTypeRepository.GetAffiliateActiveClassTypes(affiliateId).ToList();

                return View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult Edit(int scheduledClassId)
        {
            var scheduledClass = GetScheduledClassById(scheduledClassId);

            if (scheduledClass == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit scheduled class with id {scheduledClassId} was not found.");
                TempData["ErrorMessage"] = "Scheduled class not found.";
                return RedirectToAction("Index");
            }

            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);

            var instructors = _repositoryWrapper.ApplicationUserRepository.GetAffiliateActiveStaff(affiliateId);
            var classTypes = _repositoryWrapper.ClassTypeRepository.GetAffiliateActiveClassTypes(affiliateId);

            var scheduledClassViewModel = new ScheduledClassViewModel
            {
                Instructors = instructors.ToList(),
                ClassTypes = classTypes,
                ScheduledClass = scheduledClass
            };

            return View(scheduledClassViewModel);
        }

        [HttpPost]
        public IActionResult Edit(ScheduledClassViewModel viewModel)
        {
            var scheduledClass = GetScheduledClassById(viewModel.ScheduledClass.ScheduledClassId);

            if (scheduledClass == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit scheduled class with id {viewModel.ScheduledClass.ScheduledClassId} was not found.");
                TempData["ErrorMessage"] = "Scheduled class not found.";
                return RedirectToAction("Index");
            }

            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            try
            {
                if (!ModelState.IsValid)
                {
                    viewModel.Instructors = _repositoryWrapper.ApplicationUserRepository.GetAffiliateActiveStaff(affiliateId).ToList();
                    viewModel.ClassTypes = _repositoryWrapper.ClassTypeRepository.GetAffiliateActiveClassTypes(affiliateId).ToList();

                    return View(viewModel);
                }

                _logger.LogInformation($"{logPrefix} - Attempting to edit scheduled class with id {viewModel.ScheduledClass.ScheduledClassId}");

                scheduledClass.ClassTypeId = viewModel.ScheduledClass.ClassTypeId;
                scheduledClass.InstructorId = viewModel.ScheduledClass.InstructorId;
                scheduledClass.StartDateTime = viewModel.ScheduledClass.StartDateTime;
                scheduledClass.EndDateTime = viewModel.ScheduledClass.EndDateTime;
                scheduledClass.Capacity = viewModel.ScheduledClass.Capacity;

                _repositoryWrapper.ScheduledClassRepository.Update(scheduledClass);
                _repositoryWrapper.Save();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - Error editing scheduled class: {ex}");
                ModelState.AddModelError("", "Error editing scheduled class. Please try again or contact support.");

                viewModel.Instructors = _repositoryWrapper.ApplicationUserRepository.GetAffiliateActiveStaff(affiliateId).ToList();
                viewModel.ClassTypes = _repositoryWrapper.ClassTypeRepository.GetAffiliateActiveClassTypes(affiliateId).ToList();

                return View(viewModel);
            }
        }

        [HttpPut]
        public IActionResult CancelClass(int scheduledClassId, string cancellationReason)
        {
            var scheduledClass = GetScheduledClassById(scheduledClassId);

            if (scheduledClass != null)
            {
                scheduledClass.IsCancelled = true;
                scheduledClass.CancellationReason = cancellationReason;

                _repositoryWrapper.ScheduledClassRepository.Update(scheduledClass);
                _repositoryWrapper.Save();

                return Json(new { success = true, message = "Class successfully cancelled." });
            }
            else
            {
                return Json(new { success = false, message = "Unable to cancel the class." });
            }
        }

        [HttpPut]
        public async Task<IActionResult> ToggleScheduledClass(int scheduledClassId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to toggle scheduled class with id {scheduledClassId}");

            var scheduledClass = GetScheduledClassById(scheduledClassId);
            if (scheduledClass == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to toggle scheduled class with id {scheduledClassId} as could not be found.");
                return Json(new { message = "Scheduled class not found." });
            }

            try
            {
                scheduledClass.IsActive = !scheduledClass.IsActive;
                _repositoryWrapper.ScheduledClassRepository.Update(scheduledClass);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Scheduled class toggled successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to toggle scheduled class with id {scheduledClassId}: {ex}");
                return Json(new { message = "An error occurred while toggling scheduled class." });
            }
        }

        private ScheduledClass GetScheduledClassById(int scheduledClassId)
        {
            return _repositoryWrapper.ScheduledClassRepository
                .FindByCondition(sc => sc.ScheduledClassId == scheduledClassId)
                .FirstOrDefault();
        }

    }
}
