﻿using AspNetCore.Identity.Services.SendGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Cosmos.Cms.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Resend email confirmation page model
    /// </summary>
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="emailSender"></param>
        public ResendEmailConfirmationModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }
        /// <summary>
        /// Page input model
        /// </summary>
        [BindProperty] public InputModel Input { get; set; }
        /// <summary>
        /// GET method handler
        /// </summary>
        public void OnGet()
        {
        }
        /// <summary>
        /// POST method handler
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                null,
                new { userId, code },
                Request.Scheme);
            await _emailSender.SendEmailAsync(
                Input.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            ViewData["SendGridResponse"] = ((SendGridEmailSender)_emailSender).Response;
            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return Page();
        }
        /// <summary>
        /// Page input model
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Email address
            /// </summary>
            [Required][EmailAddress] public string Email { get; set; }
        }
    }
}