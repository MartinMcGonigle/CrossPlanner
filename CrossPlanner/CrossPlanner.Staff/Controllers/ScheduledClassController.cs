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
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ScheduledClassController(
            ILogger<ScheduledClassController> logger,
            IRepositoryWrapper repositoryWrapper,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _userManager = userManager;
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

            _logger.LogInformation($"{logPrefix} - Attempting to display scheduled classes for affiliate with id {affiliateId} on {selectedDate.ToShortDateString()}");

            var scheduledClassDetails = _repositoryWrapper.ScheduledClassRepository.GetAffiliateScheduledClassByDateStaff(affiliateId, selectedDate);

            var viewModel = new DailyScheduleViewModel
            {
                SelectedDate = selectedDate,
                ScheduledClassDetails = scheduledClassDetails.ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult WhosComing(int scheduledClassId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to display who's coming to scheduled class with id {scheduledClassId}");

            var scheduledClass = _repositoryWrapper.ScheduledClassRepository.GetScheduledClassById(scheduledClassId);
            if (scheduledClass == null)
            {
                _logger.LogWarning($"{logPrefix} - Scheduled class with id {scheduledClassId} not found.");
                return NotFound();
            }

            var attendees = _repositoryWrapper.ScheduledClassReservationRepository.GetClassAttendeesByScheduledClassId(scheduledClassId);

            var viewModel = new WhosComingViewModel
            {
                ScheduledClassId = scheduledClassId,
                ClassName = scheduledClass.ClassType.Title,
                StartDateTime = scheduledClass.StartDateTime,
                EndDateTime = scheduledClass.EndDateTime,
                Attendees = attendees
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
               // scheduledClass.InstructorId = viewModel.ScheduledClass.InstructorId;
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

        [HttpPut]
        public async Task<IActionResult> MarkAbsent(int scheduledClassReservationId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to mark class reservation {scheduledClassReservationId} as absent");

            var scheduledClassReservation = GetScheduledClassReservationById(scheduledClassReservationId);
            if (scheduledClassReservation == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to mark class reservation {scheduledClassReservationId} as absent because it could not be found");
                return Json(new { message = "Class reservation not found." });
            }

            try
            {
                scheduledClassReservation.IsPresent = false;
                _repositoryWrapper.ScheduledClassReservationRepository.Update(scheduledClassReservation);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Class reservation marked as absent." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to mark class reservation as absent {scheduledClassReservationId}: {ex}");
                return Json(new { message = "An error occurred while marking class reservation as absent." });
            }
        }

        [HttpPut]
        public async Task<IActionResult> MarkPresent(int scheduledClassReservationId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to mark class reservation {scheduledClassReservationId} as present");

            var scheduledClassReservation = GetScheduledClassReservationById(scheduledClassReservationId);
            if (scheduledClassReservation == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to mark class reservation {scheduledClassReservationId} as present because it could not be found");
                return Json(new { message = "Class reservation not found." });
            }

            try
            {
                scheduledClassReservation.IsPresent = true;
                _repositoryWrapper.ScheduledClassReservationRepository.Update(scheduledClassReservation);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Class reservation marked as present." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to mark class reservation as present {scheduledClassReservationId}: {ex}");
                return Json(new { message = "An error occurred while marking class reservation as present." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreservedMembers (int scheduledClassId)
        {
            try
            {
                Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);

                if (affiliateId == 0)
                {
                    _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId}");
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                var reservedMemberIds = GetReservedMemberIds(scheduledClassId);
                
                var unreservedMembers = _repositoryWrapper.MembershipRepository
                    .FindByCondition(m => m.IsActive && m.MembershipPlan.AffiliateId == affiliateId && !reservedMemberIds.Contains(m.MemberId))
                    .Select(m => new
                    {
                        m.MemberId,
                        m.Member.FirstName,
                        m.Member.LastName,
                        m.Member.Email,
                        m.MembershipId,
                        scheduledClassId,
                    })
                    .ToList();

                return Ok(unreservedMembers);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - Unable to retrieve unreserved members for class with id {scheduledClassId}: {ex.Message}", ex);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReserveScheduledClass(int scheduledClassId, int membershipId)
        {
            try
            {
                _logger.LogInformation($"{logPrefix} - Attempting to add attendee to scheduled class with id {scheduledClassId} with membership id {membershipId}");
                
                var existingMembership = GetMembershipById(membershipId);
                if (existingMembership == null)
                {
                    _logger.LogWarning($"{logPrefix} - Unable to reserve scheduled class with id {scheduledClassId} as could not find membership with id {membershipId}");
                    return Json(new { message = "Membership not found." });
                }

                var scheduledClass = GetScheduledClassById(scheduledClassId);
                if (scheduledClass == null)
                {
                    _logger.LogWarning($"{logPrefix} - Unable to reserve scheduled class with id {scheduledClassId} as could not be found.");
                    return Json(new { message = "Class not found." });
                }

                var scheduledClassReservation = new ScheduledClassReservation
                {
                    MembershipId = membershipId,
                    ScheduledClassId = scheduledClassId,
                    ReservationDate = DateTime.Now,
                    IsPresent = true
                };

                _repositoryWrapper.ScheduledClassReservationRepository.Create(scheduledClassReservation);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Scheduled class reserved successfully." });
            }
            catch(Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to reserve scheduled class with id {scheduledClassId} for membership id {membershipId}: {ex}");
                return Json(new { message = "An error occurred while adding reservation." });
            }
        }

        private List<string> GetReservedMemberIds(int scheduledClassId)
        {
            return _repositoryWrapper.ScheduledClassReservationRepository
                .FindByCondition(scr => scr.ScheduledClassId == scheduledClassId)
                .Select(scr => scr.Membership.MemberId)
                .ToList();
        }

        private ScheduledClassReservation GetScheduledClassReservationById(int scheduledClassReservationId)
        {
            return _repositoryWrapper.ScheduledClassReservationRepository
                .FindByCondition(scr => scr.ScheduledClassReservationId == scheduledClassReservationId)
                .FirstOrDefault();
        }

        private ScheduledClass GetScheduledClassById(int scheduledClassId)
        {
            return _repositoryWrapper.ScheduledClassRepository
                .FindByCondition(sc => sc.ScheduledClassId == scheduledClassId)
                .FirstOrDefault();
        }

        private Membership? GetMembershipById(int membershipId)
        {
            return _repositoryWrapper.MembershipRepository
                .FindByCondition(m => m.MembershipId == membershipId)
                .FirstOrDefault();
        }
    }
}
