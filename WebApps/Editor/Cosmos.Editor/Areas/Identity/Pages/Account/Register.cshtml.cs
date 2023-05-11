using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Cms.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Cosmos.Cms.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Register page model
    /// </summary>
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IOptions<SiteSettings> _options;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="roleManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="logger"></param>
        /// <param name="emailSender"></param>
        /// <param name="options"></param>
        public RegisterModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IOptions<SiteSettings> options)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _options = options;
        }
        /// <summary>
        /// Page input model
        /// </summary>
        [BindProperty] public InputModel Input { get; set; }
        /// <summary>
        /// Return URL
        /// </summary>
        public string ReturnUrl { get; set; }
        /// <summary>
        /// External logins
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        /// <summary>
        /// GET method handler
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            return Page();
        }
        /// <summary>
        /// POST method handler
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Upon setup with first user, make that person an admin.
                    var newAdministrator = await Ensure_RolesAndAdmin_Exists(user);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        null,
                        new { area = "Identity", userId = user.Id, code, returnUrl },
                        Request.Scheme);

                    if (!newAdministrator)
                    {
                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    }

                    await _signInManager.SignInAsync(user, false);

                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        /// <summary>
        /// Ensures the required roles exist, and, add the first user as an administrator.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>True if a new administrator was created.</returns>
        private async Task<bool> Ensure_RolesAndAdmin_Exists(IdentityUser user)
        {

            foreach (var role in RequiredIdentityRoles.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var identityRole = new IdentityRole(role);
                    var result = await _roleManager.CreateAsync(identityRole);
                    if (!result.Succeeded)
                    {
                        var error = result.Errors.FirstOrDefault();
                        var exception = new Exception($"Code: {error.Code} - {error.Description}");
                        _logger.LogError(exception.Message);
                        throw exception;
                    }
                }
            }

            var userCount = await _userManager.Users.CountAsync();

            // If there is only one registered user (the person who just registered for instance),
            // and that person is not in the Administrators role, then add that person now.
            // There must be at least one administrator.
            if (userCount == 1 && (await _userManager.IsInRoleAsync(user, RequiredIdentityRoles.Administrators)) == false)
            {
                var result = await _userManager.AddToRoleAsync(user, RequiredIdentityRoles.Administrators);

                if (result.Succeeded)
                {

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmResult = await _userManager.ConfirmEmailAsync(user, code);

                    if (confirmResult.Succeeded)
                    {
                        _logger.LogInformation($"{user.Email} added to the {RequiredIdentityRoles.Administrators} role.");
                    }
                    else
                    {
                        var error = result.Errors.FirstOrDefault();
                        var exception = new Exception($"Code: {error.Code} - {error.Description}");
                        _logger.LogError(exception.Message);
                        throw exception;
                    }
                }
                else
                {
                    var error = result.Errors.FirstOrDefault();
                    var exception = new Exception($"Code: {error.Code} - {error.Description}");
                    _logger.LogError(exception.Message);
                    throw exception;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Post model
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Email address
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
            /// <summary>
            /// Password
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
            /// <summary>
            /// Password is confirmed
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
    }
}