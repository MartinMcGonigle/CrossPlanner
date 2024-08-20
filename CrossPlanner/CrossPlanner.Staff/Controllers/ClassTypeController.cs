using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrossPlanner.Repository.Wrapper;
using CrossPlanner.Domain.Models;

namespace CrossPlanner.Staff.Controllers
{
    [Authorize(Roles = "SuperUser,Manager")]
    public class ClassTypeController : Controller
    {
        private readonly ILogger<ClassTypeController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private const string logPrefix = "Ctlr|ClassType";

        public ClassTypeController(
            ILogger<ClassTypeController> logger,
            IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            _logger.LogInformation($"{logPrefix} - Attempting to display class types of affiliate with id {affiliateId}");

            var data = _repositoryWrapper.ClassTypeRepository
                .FindByCondition(ct => !ct.IsDeleted
                && ct.AffiliateId == affiliateId)
                .ToList();
            
            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ClassType model)
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            model.AffiliateId = affiliateId;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _logger.LogInformation($"{logPrefix} - Attempting to create class type for affiliate with id {affiliateId}");
                _repositoryWrapper.ClassTypeRepository.Create(model);
                _repositoryWrapper.Save();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - Error creating class type: {ex}");
                ModelState.AddModelError("", "Error creating class type: Please try again or contact support.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Edit(int classTypeId)
        {
            var classType = GetClassTypeById(classTypeId);

            if (classType == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit class type with id {classTypeId} was not found.");
                TempData["ErrorMessage"] = "Class type not found.";
                return RedirectToAction("Index");
            }

            return View(classType);
        }

        [HttpPost]
        public IActionResult Edit(ClassType model)
        {
            var classType = GetClassTypeById(model.ClassTypeId);

            if (classType == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to edit class type with id {model.ClassTypeId} was not found.");
                TempData["ErrorMessage"] = "Class type not found.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _logger.LogInformation($"{logPrefix} - Attempting to edit class type with id {model.ClassTypeId}");

                classType.Title = model.Title;

                _repositoryWrapper.ClassTypeRepository.Update(classType);
                _repositoryWrapper.Save();

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                _logger.LogError($"{logPrefix} - Error editing class type: {ex}");
                ModelState.AddModelError("", "Error updating class type: Please try again or contact support.");
                return View(model);
            }
        }

        [HttpPut]
        public async Task<IActionResult> ToggleClassType(int classTypeId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to toggle class type with id {classTypeId}");

            var classType = GetClassTypeById(classTypeId);
            if (classType == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to toggle class type with id {classTypeId} as class type could not be found");
                return Json(new { message = "Class type not found." });
            }

            try
            {
                classType.IsActive = !classType.IsActive;
                _repositoryWrapper.ClassTypeRepository.Update(classType);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Class type toggled successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to toggle class type with id {classTypeId}: {ex}");
                return Json(new { message = "An error occurred while toggling class type." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteClassType(int classTypeId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to delete class type with id {classTypeId}");

            var classType = GetClassTypeById(classTypeId);
            if (classType == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to delete class type with id {classTypeId} as class type could not be found");
                return Json(new { message = "Class type not found." });
            }

            try
            {
                classType.IsDeleted = !classType.IsDeleted;
                _repositoryWrapper.ClassTypeRepository.Update(classType);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Class type deleted successfully." });
            }
            catch(Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to delete class type with id {classTypeId}: {ex}");
                return Json(new { message = "An error occurred while deleting class type." });
            }
        }

        private ClassType GetClassTypeById(int classTypeId)
        {
            return _repositoryWrapper.ClassTypeRepository
                .FindByCondition(x => x.ClassTypeId == classTypeId)
                .FirstOrDefault();
        } 
    }
}