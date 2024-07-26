using CrossPlanner.Domain.OtherModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrossPlanner.Member.Controllers
{
    public class BaseController : Controller
    {
        internal readonly IHttpContextAccessor _httpContextAccessor;
        internal string _userId;

        public BaseController(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                _userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [ViewData]
        public LinkedList<StepViewModel> Steps { get; set; }

        [ViewData]
        public StepViewModel CurrentStep
        {
            get
            {
                return Steps.SingleOrDefault(s => s.Name == ControllerContext.RouteData.Values["action"].ToString());
            }
        }

        [ViewData]
        public StepViewModel NextStep
        {
            get
            {
                var step = CurrentStep;

                if (step == null)
                {
                    return Steps.First.Value;
                }

                var currentNode = Steps.Find(step);

                return currentNode?.Next?.Value;
            }
        }

        [ViewData]
        public int CurrentStepNumber
        {
            get
            {
                if (CurrentStep == null)
                {
                    return 0;
                }

                var counter = 0;

                foreach (var step in Steps)
                {
                    counter++;

                    if (CurrentStep.Equals(step))
                    {
                        break;
                    }
                }

                return counter;
            }
        }

        [ViewData]
        public StepViewModel PreviousStep
        {
            get
            {
                var step = CurrentStep;

                if (step == null)
                {
                    return Steps.First.Value;
                }

                var currentNode = Steps.Find(step);
                if (currentNode != null && currentNode.Equals(Steps.First))
                {
                    return null;
                }

                return currentNode?.Previous?.Value;
            }
        }
    }
}
