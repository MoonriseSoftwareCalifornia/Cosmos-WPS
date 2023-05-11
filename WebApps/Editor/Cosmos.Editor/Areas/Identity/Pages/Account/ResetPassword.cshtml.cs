using Cosmos.Cms.Common.Services.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Cms.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Reset password page model
    /// </summary>
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly IOptions<SiteSettings> _options;
        private readonly UserManager<IdentityUser> _userManager;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="options"></param>
        public ResetPasswordModel(UserManager<IdentityUser> userManager, IOptions<SiteSettings> options)
        {
            _userManager = userManager;
            _options = options;
        }
        /// <summary>
        /// Input model
        /// </summary>
        [BindProperty] public InputModel Input { get; set; }
        /// <summary>
        /// Get handler
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public IActionResult OnGet(string code = null)
        {
            if (code == null) return BadRequest("A code must be supplied for password reset.");

            Input = new InputModel
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
            };
            return Page();
        }
        /// <summary>
        /// Post handler
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded) return RedirectToPage("./ResetPasswordConfirmation");

            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }
        /// <summary>
        /// Form input model
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// User email address
            /// </summary>
            [Required][EmailAddress] public string Email { get; set; }
            /// <summary>
            /// Error message (if any)
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            /// <summary>
            /// Confirm password field
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            /// <summary>
            /// Code field
            /// </summary>
            public string Code { get; set; }
        }
    }
}