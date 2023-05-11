using Cosmos.Cms.Data;
using Cosmos.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// CKEditor post model
    /// </summary>
    public class HtmlEditorSignal
    {
        /// <summary>
        ///     Article title
        /// </summary>
        [MaxLength(80)]
        [StringLength(80)]
        [Display(Name = "Article title")]
        [ArticleTitleValidation]
        [Remote("CheckTitle", "Edit", AdditionalFields = "ArticleNumber")]
        public string Title { get; set; }

        /// <summary>
        ///     Roles allowed to view this page.
        /// </summary>
        /// <remarks>If this value is null, it assumes page can be viewed anonymously.</remarks>
        public string RoleList { get; set; }

        /// <summary>
        ///     Date and time of when this was published
        /// </summary>
        [Display(Name = "Publish on date/time (PST):")]
        [DataType(DataType.DateTime)]
        [DateTimeUtcKind]
        public DateTimeOffset? Published { get; set; }

        /// <summary>
        ///     Date and time of when this was updated
        /// </summary>
        [Display(Name = "Updated on date/time (PST):")]
        [DataType(DataType.DateTime)]
        [DateTimeUtcKind]
        public DateTimeOffset? Updated { get; set; }

        /// <summary>
        /// Article banner image
        /// </summary>
        public string BannerImage { get; set; }

        #region ITEMS RETURNED AFTER SAVE (/Editor/PostRegions)

        /// <summary>
        /// URL path returned after save
        /// </summary>
        public string UrlPath { get; set; }

        /// <summary>
        /// Version number returned after save
        /// </summary>
        public int VersionNumber { get; set; }

        #endregion
    }

}
