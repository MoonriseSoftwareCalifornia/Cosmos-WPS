using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Common.Data
{
    /// <summary>
    /// Article list item
    /// </summary>
    public class CatalogEntry
    {
        /// <summary>
        ///     Article number
        /// </summary>
        [Key]
        [Display(Name = "Article#")]
        public int ArticleNumber { get; set; }

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
        public DateTimeOffset Updated { get; set; }

        /// <summary>
        ///     Date and time of when this item was published, and made public
        /// </summary>
        [Display(Name = "Publish date/time")]
        public DateTimeOffset? Published { get; set; }

        /// <summary>
        ///     Url of this item
        /// </summary>
        [Display(Name = "Url")]
        public string UrlPath { get; set; }
    }
}
