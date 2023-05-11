using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Common.Data
{
    /// <summary>
    /// Published article or page
    /// </summary>
    public class PublishedPage
    {

        /// <summary>
        ///     Unique article entity primary key number (not to be confused with article number)
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        ///     Article number
        /// </summary>
        /// <remarks>An article number.</remarks>
        public int ArticleNumber { get; set; }

        /// <summary>
        ///     Status of this article
        /// </summary>
        /// <remarks>See <see cref="StatusCodeEnum" /> enum for code numbers.</remarks>
        public int StatusCode { get; set; } = 0;

        /// <summary>
        ///     This is the URL of the article.
        /// </summary>
        [MaxLength(128)]
        [StringLength(128)]
        public string UrlPath { get; set; }

        /// <summary>
        /// Sets the parent URL page
        /// </summary>
        [Display(Name = "Parent URL Path")]
        public string ParentUrlPath { get;set; }

        /// <summary>
        ///     Version number of the article.
        /// </summary>
        [Display(Name = "Article version")]
        public int VersionNumber { get; set; }

        /// <summary>
        ///     Date/time of when this article is published.
        /// </summary>
        /// <remarks>Null value here means this article is not published.</remarks>
        [Display(Name = "Publish on (UTC):")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset? Published { get; set; }

        /// <summary>
        ///     If set, is the date/time when this version of the article expires.
        /// </summary>
        [Display(Name = "Expires on (UTC):")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset? Expires { get; set; }

        /// <summary>
        ///     Title of the article
        /// </summary>
        [MaxLength(254)]
        [StringLength(254)]
        [Display(Name = "Article title")]
        public string Title { get; set; }

        /// <summary>
        ///     HTML content of the page.
        /// </summary>
        [DataType(DataType.Html)]
        public string Content { get; set; }

        /// <summary>
        ///     Date/time of when this article was last updated.
        /// </summary>
        [Display(Name = "Article last saved")]
        public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Article banner image
        /// </summary>
        [DataType(DataType.ImageUrl)]
        [Required(AllowEmptyStrings = true)]
        public string BannerImage { get; set; }

        /// <summary>
        ///     JavaScript injected into the header of the web page.
        /// </summary>
        [DataType(DataType.Html)]
        public string HeaderJavaScript { get; set; }

        /// <summary>
        ///     JavaScript injected into the footer of the web page.
        /// </summary>
        [DataType(DataType.Html)]
        public string FooterJavaScript { get; set; }

        /// <summary>
        /// Information about the person who authored this document.
        /// </summary>
        public string AuthorInfo { get; set; }

        #region PERMISSIONS

        /// <summary>
        ///     A comma delimited list of roles that can access this article. If blank the assumption is anonymous access.
        /// </summary>
        [MaxLength(512)]
        public string RoleList { get; set; } = string.Empty;

        #endregion
    }
}
