using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cosmos.Cms.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Reset password confirmation page model
    /// </summary>
    [AllowAnonymous]
    public class ResetPasswordConfirmationModel : PageModel
    {
        /// <summary>
        /// GET method handler
        /// </summary>
        public void OnGet()
        {
            // Method intentionally left empty.
        }
    }
}