using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CrossPlanner.Staff.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationLinkModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}