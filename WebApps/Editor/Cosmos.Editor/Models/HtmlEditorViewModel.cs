using Cosmos.Common.Models;
using Cosmos.Cms.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    ///     Article edit model returned when an article has been saved.
    /// </summary>
    public class HtmlEditorViewModel
    {

        /// <summary>
        ///     Constructor
        /// </summary>
        public HtmlEditorViewModel()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="model"></param>
        public HtmlEditorViewModel(ArticleViewModel model)
        {
            Id = model.Id;
            ArticleNumber = model.ArticleNumber;
            UrlPath = model.UrlPath;
            VersionNumber = model.VersionNumber;
            this.Published = model.Published;
            Title = model.Title;
            Content = model.Content;
            RoleList = model.RoleList;
            Updated = model.Updated;
            BannerImage = model.BannerImage;
        }

        /// <summary>
        ///     Entity key for the article
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// When saving, save as new version
        /// </summary>
        public bool SaveAsNewVersion { get; set; } = false;

        /// <summary>
        ///     Article number
        /// </summary>
        public int ArticleNumber { get; set; }

        /// <summary>
        ///     Url of this page
        /// </summary>
        [MaxLength(128)]
        [StringLength(128)]
        public string UrlPath { get; set; }

        /// <summary>
        ///     Version number of this article
        /// </summary>
        [Display(Name = "Article version")]
        public int VersionNumber { get; set; }

        /// <summary>
        ///     Article title
        /// </summary>
        [MaxLength(80)]
        [StringLength(80)]
        [ArticleTitleValidation]
        [Display(Name = "Article title")]
        [Remote("CheckTitle", "Edit", AdditionalFields = "ArticleNumber")]
        public string Title { get; set; }

        /// <summary>
        ///     HTML Content of the page
        /// </summary>
        [DataType(DataType.Html)]
        public string Content { get; set; }

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
        [Display(Name = "Publish on date/time (PST):")]
        [DataType(DataType.DateTime)]
        [DateTimeUtcKind]
        public DateTimeOffset? Updated { get; set; }

        /// <summary>
        /// Article banner image
        /// </summary>
        public string BannerImage { get; private set; }

        /// <summary>
        /// Update existing version
        /// </summary>
        public bool UpdateExisting { get; set; } = true;
    }
}