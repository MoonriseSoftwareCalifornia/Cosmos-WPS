using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    ///     Article list item used primarily on page list page
    /// </summary>
    public class ArticleListItem
    {

        /// <summary>
        ///     Indicates if this is the "Home" page
        /// </summary>
        [Display(Name = "Is home page?")]
        public bool IsDefault { get; set; } = false;

        /// <summary>
        ///     Article number
        /// </summary>
        [Display(Name = "Article#")]
        [Key]
        public int ArticleNumber { get; set; }

        /// <summary>
        ///     Version number of the article number
        /// </summary>
        [Display(Name = "Version")]
        public int VersionNumber { get; set; }

        /// <summary>
        ///     Title of the page, also used as the basis for the URL
        /// </summary>
        [Display(Name = "Title")]
        public string Title { get; set; }

        /// <summary>
        ///     Disposition of the page
        /// </summary>
        [Display(Name = "Status")]
        public string Status { get; set; }

        /// <summary>
        ///     Date/time of when this page was last updated
        /// </summary>
        [Display(Name = "Updated")]
        public DateTimeOffset Updated { get; internal set; }

        /// <summary>
        ///     Date and time of when this item was published, and made public
        /// </summary>
        [Display(Name = "Publish date/time")]
        public DateTimeOffset? LastPublished { get; set; }

        /// <summary>
        ///     Url of this item
        /// </summary>
        [Display(Name = "Url")]
        public string UrlPath { get; set; }

    }
}