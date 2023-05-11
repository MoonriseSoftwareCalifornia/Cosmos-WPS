using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cosmos.Cms.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// Show recover codes page model
    /// </summary>
    public class ShowRecoveryCodesModel : PageModel
    {
        /// <summary>
        /// Recovery codes
        /// </summary>
        [TempData] public string[] RecoveryCodes { get; set; }
        /// <summary>
        /// Status message
        /// </summary>
        [TempData] public string StatusMessage { get; set; }
        /// <summary>
        /// Get handler
        /// </summary>
        /// <returns></returns>
        public IActionResult OnGet()
        {
            if (RecoveryCodes == null || RecoveryCodes.Length == 0) return RedirectToPage("./TwoFactorAuthentication");

            return Page();
        }
    }
}