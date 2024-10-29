using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrossPlanner.Repository.Wrapper;
using CrossPlanner.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CrossPlanner.Member.Controllers
{
    [Authorize(Roles = "Member")]
    public class ReservedClassController : Controller
    {
        private readonly ILogger<ReservedClassController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private const string logPrefix = "Ctlr|ReservedClass";

        public ReservedClassController(
            ILogger<ReservedClassController> logger,
            IRepositoryWrapper repositoryWrapper,
            SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Int32.TryParse(User.FindFirst("Affiliate")?.Value, out int affiliateId);
            var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (affiliateId == 0 || string.IsNullOrEmpty(memberId))
            {
                _logger.LogWarning($"{logPrefix} - Redirecting user to login page as affiliateId was {affiliateId} and memberId was {memberId}");
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var existingMembership = _repositoryWrapper.MembershipRepository.GetMembershipByAffiliateMemberId(affiliateId, memberId);
            if (existingMembership == null)
            {
                TempData["MembershipMessage"] = "You do not have an active membership. Please activate a membership to book into classes.";
            }

            var dateTime = DateTime.Now;
            
            var scheduledClassReservations = _repositoryWrapper.ScheduledClassReservationRepository
                .FindByCondition(scr =>
                scr.Membership.MemberId == memberId &&
                scr.Membership.IsActive &&
                scr.Membership.MembershipPlan.AffiliateId == affiliateId &&
                scr.ScheduledClass.StartDateTime > dateTime &&
                scr.ScheduledClass.IsActive == true &&
                scr.ScheduledClass.IsCancelled != true &&
                scr.ScheduledClass.IsDeleted != true &&
                scr.ScheduledClass.ClassType.AffiliateId == affiliateId)
                .Include(scr => scr.ScheduledClass.ClassType.Affiliate)
                .Include(scr => scr.ScheduledClass.Instructor)
                .OrderBy(scr => scr.ScheduledClass.StartDateTime)
                .ToList();

            return View(scheduledClassReservations);
        }
    }
}