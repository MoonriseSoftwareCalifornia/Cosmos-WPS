using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace Cosmos.Cms.Areas.Identity.Pages
{
    /// <summary>
    /// Error page model
    /// </summary>
    [AllowAnonymous]
    [ResponseCache(NoStore = true)]
    public class ErrorModel : PageModel
    {
        /// <summary>
        /// Request ID
        /// </summary>
        public string RequestId { get; set; }
        /// <summary>
        /// Show request ID
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        /// <summary>
        /// GET method handler
        /// </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}