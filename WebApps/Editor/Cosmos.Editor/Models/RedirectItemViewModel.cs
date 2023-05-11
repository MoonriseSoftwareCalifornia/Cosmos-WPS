using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Redirect from and to URL item
    /// </summary>
    public class RedirectItemViewModel
    {
        /// <summary>
        /// Redirect ID
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Redirect from this URL (local to this web server)
        /// </summary>
        [RedirectUrl]
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Redirect from URL")]
        public string FromUrl { get; set; }

        /// <summary>
        /// Redirect to this URL
        /// </summary>
        [RedirectUrl]
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Redirect to URL")]
        public string ToUrl { get; set; }
    }
}
