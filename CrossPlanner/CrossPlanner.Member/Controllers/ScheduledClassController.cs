using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrossPlanner.Repository.Wrapper;
using CrossPlanner.Domain.OtherModels;
using CrossPlanner.Domain.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using CrossPlanner.Domain.Enums;

namespace CrossPlanner.Member.Controllers
{
    [Authorize(Roles = "Member")]
    public class ScheduledClassController : Controller
    {
        private readonly ILogger<ScheduledClassController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private const string logPrefix = "Ctlr|ScheduledClass";

        public ScheduledClassController(
            ILogger<ScheduledClassController> logger,
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
            var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var selectedDate = date ?? DateTime.Today;

            if (affiliateId == 0 || string.IsNullOrEmpty(memberId))
            {
                _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId} and memberId was {memberId}");
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" } );
            }

            _logger.LogInformation($"{logPrefix} - Attempting to display scheduled classes for affiliate with id {affiliateId} on {selectedDate.ToShortDateString()}");

            var existingMembership = _repositoryWrapper.MembershipRepository.GetMembershipByAffiliateMemberId(affiliateId, memberId);
            if (existingMembership == null)
            {
                TempData["MembershipMessage"] = "You do not have an active membership. Please activate a membership to book into classes.";
            }

            var scheduledClassDetails = _repositoryWrapper.ScheduledClassRepository.GetAffiliateScheduledClassByDateMember(affiliateId, selectedDate, memberId);

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

        [HttpPost]
        public async Task<IActionResult> ReserveScheduledClass(int scheduledClassId)
        {
            try
            {
                Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
                var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (affiliateId == 0 || string.IsNullOrEmpty(memberId))
                {
                    _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId} and memberId was {memberId}");
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }
                
                _logger.LogInformation($"{logPrefix} - Member with id {memberId} is attempting to reserve scheduled class with id {scheduledClassId}");
                
                var existingMembership = _repositoryWrapper.MembershipRepository.GetMembershipByAffiliateMemberId(affiliateId, memberId);
                if (existingMembership == null)
                {
                    _logger.LogWarning($"{logPrefix} - Unable to reserve scheduled class with id {scheduledClassId} as member with id {memberId} does not have an active membership");
                    return Json(new { message = "You do not have an active membership." });
                }
                
                var scheduledClass = _repositoryWrapper.ScheduledClassRepository.GetScheduledClassByIdSP(scheduledClassId);
                if (scheduledClass == null)
                {
                    _logger.LogWarning($"{logPrefix} - Unable to reserve scheduled class with id {scheduledClassId} as could not be found.");
                    return Json(new { message = "Class not found." });
                }

                if (scheduledClass.StartDateTime > existingMembership.EndDate)
                {
                    // This rule will change when we implement auto renew we will implement this at a later date
                    _logger.LogWarning($"{logPrefix} - membership with id {existingMembership.MembershipId} will not be active for the scheduled class on {scheduledClass.StartDateTime} due to the membership's end date {existingMembership.EndDate}.");
                    return Json(new { message = "Your membership will not be active at the time of the scheduled class." });
                }

                if (string.Equals(Enum.GetName(typeof(MembershipType), existingMembership.MembershipPlan.Type), Enum.GetName(MembershipType.Weekly)))
                {
                    var startOfWeek = scheduledClass.StartDateTime.AddDays(-(int)scheduledClass.StartDateTime.DayOfWeek).Date;
                    var endOfWeek = startOfWeek.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);

                    // Adjust start and end of week based on membership start and end dates
                    if (existingMembership.StartDate > startOfWeek)
                    {
                        startOfWeek = existingMembership.StartDate;
                    }

                    if (existingMembership.EndDate < endOfWeek)
                    {
                        endOfWeek = (DateTime)existingMembership.EndDate;
                    }

                    var activeDaysThisWeek = (endOfWeek - startOfWeek).Days + 1; // +1 because both start and end days are inclusive

                    // Calculate the adjusted number of classes allowed for this week
                    var maxClassesThisWeek = Math.Ceiling((decimal)((decimal)activeDaysThisWeek / 7 * existingMembership.MembershipPlan.NumberOfClasses));

                    var classesThisWeek = _repositoryWrapper.ScheduledClassReservationRepository.GetMemberClassAttendanceByWeek(existingMembership.MembershipId, startOfWeek, endOfWeek);

                    if (classesThisWeek >= maxClassesThisWeek)
                    {
                        _logger.LogWarning($"{logPrefix} - Member with id {memberId} has reached their weekly class limit. Unable to reserve scheduled class with id {scheduledClassId}.");
                        return Json(new { message = $"You have reached your weekly limit for class reservations: {classesThisWeek} out of {maxClassesThisWeek} classes reserved." });
                    }
                }
                else if (string.Equals(Enum.GetName(typeof(MembershipType), existingMembership.MembershipPlan.Type), Enum.GetName(MembershipType.Monthly)))
                {
                    var classAttendancewithMembership = _repositoryWrapper.ScheduledClassReservationRepository.GetMemberClassAttendanceByMembershipId(existingMembership.MembershipId);

                    if (classAttendancewithMembership >= existingMembership.MembershipPlan.NumberOfClasses)
                    {
                        _logger.LogWarning($"{logPrefix} - Member with id {memberId} has reached their monthly class limit. Unable to reserve scheduled class with id {scheduledClassId}.");
                        return Json(new { message = $"You have reached your monthly limit for class reservations: {existingMembership.MembershipPlan.NumberOfClasses} classes over {existingMembership.MembershipPlan.NumberOfMonths} months." });
                    }
                }

                if (scheduledClass.Capacity != null && scheduledClass.ReservationsCount >= scheduledClass.Capacity)
                {
                    _logger.LogWarning($"{logPrefix} - Unable to reserve scheduled class with id {scheduledClassId} as it has reached its capacity.");
                    return Json(new { message = "Class is full." });
                }

                var scheduledClassReservation = new ScheduledClassReservation
                {
                    MembershipId = existingMembership.MembershipId,
                    ScheduledClassId = scheduledClassId,
                    ReservationDate = DateTime.Now,
                };

                _repositoryWrapper.ScheduledClassReservationRepository.Create(scheduledClassReservation);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Scheduled class reserved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to reserve scheduled class with id {scheduledClassId}: {ex}");
                return Json(new { message = "An error occurred while reserving class." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveReservationScheduledClass(int scheduledClassReservationId)
        {
            _logger.LogInformation($"{logPrefix} - Attempting to delete scheduled class reservation with id {scheduledClassReservationId}");

            var scheduledClassReservation = GetScheduledClassReservationById(scheduledClassReservationId);
            if (scheduledClassReservation == null)
            {
                _logger.LogWarning($"{logPrefix} - Unable to delete scheduled class reservation with id {scheduledClassReservationId} as could not be found");
                return Json(new { message = "Reservation not found." });
            }

            try
            {
                _repositoryWrapper.ScheduledClassReservationRepository.Delete(scheduledClassReservation);
                await _repositoryWrapper.SaveAsync();

                return Json(new { message = "Successfully removed reservation." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix} - An error occurred whilst attempting to delete scheduled class reservation with id {scheduledClassReservationId}: {ex}");
                return Json(new { message = "An error occurred while removing reservation." });
            }
        }

        private ScheduledClassReservation GetScheduledClassReservationById(int scheduledClassReservationId)
        {
            return _repositoryWrapper.ScheduledClassReservationRepository
                .FindByCondition(scr => scr.ScheduledClassReservationId == scheduledClassReservationId)
                .FirstOrDefault();
        }
    }
}