using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Common.Services.Configurations
{
    /// <summary>
    ///     SendGrid Authentication Options
    /// </summary>
    public class SendGridConfig
    {
        /// <summary>
        ///     SendGrid key
        /// </summary>
        [Required]
        [Display(Name = "SendGrid key")]
        public string SendGridKey { get; set; }

        /// <summary>
        ///     From Email address
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "From email address")]
        public string EmailFrom { get; set; }
    }
}