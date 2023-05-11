using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Users index view model
    /// </summary>
    public class UserIndexViewModel
    {
        /// <summary>
        /// Unique user ID
        /// </summary>
        [Key] public string UserId { get; set; }
        /// <summary>
        /// User's email address
        /// </summary>
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string EmailAddress { get; set; }

        /// <summary>
        /// User's email address is confirmed.
        /// </summary>
        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// User's phone number (can be SMS)
        /// </summary>
        [Display(Name = "Telephone #")]
        [Phone]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// User's phone number (can be SMS)
        /// </summary>
        [Display(Name = "Phone Confirmed")]
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// User has two factor authentication enabled
        /// </summary>
        [Display(Name = "2FA Enabled")]
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// Account is locked out
        /// </summary>
        [Display(Name = "LockedOut?")]
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Role memebership
        /// </summary>
        public List<string> RoleMembership { get; set; } = new List<string>();
    }
}
