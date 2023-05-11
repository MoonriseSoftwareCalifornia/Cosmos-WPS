using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Article duplication model
    /// </summary>
    public class DuplicateViewModel
    {
        /// <summary>
        /// Entity ID
        /// </summary>
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// ID of the Article
        /// </summary>
        public int ArticleId { get; set; }

        /// <summary>
        /// ArticleVersion
        /// </summary>
        public int ArticleVersion { get; set; }

        /// <summary>
        /// Parent page (optional)
        /// </summary>
        [Display(Name = "Parent page:")]
        public string ParentPageTitle { get; set; }

        /// <summary>
        /// New page title
        /// </summary>
        [Display(Name = "New page title:")]
        [Required(AllowEmptyStrings = false)]
        public string Title { get; set; }

        /// <summary>
        /// Optional date/time when published
        /// </summary>
        [Display(Name = "Published date/time:")]
        public DateTimeOffset? Published { get; set; }

    }

}
