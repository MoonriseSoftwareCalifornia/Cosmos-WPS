using Cosmos.Common.Models;
using Cosmos.Cms.Data;
using Cosmos.Cms.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Edit code post view model
    /// </summary>
    public class EditCodePostModel : ICodeEditorViewModel
    {
        /// <summary>
        /// Article ID
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// When saving, save as new version
        /// </summary>
        public bool SaveAsNewVersion { get; set; } = false;

        /// <summary>
        /// Article number
        /// </summary>
        public int ArticleNumber { get; set; }

        /// <summary>
        /// Article 
        /// </summary>
        [MaxLength(80)]
        [StringLength(80)]
        [ArticleTitleValidation]
        [Display(Name = "Article title")]
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
        public virtual DateTimeOffset? Published { get; set; }

        /// <summary>
        ///     Date and time of when this was updated
        /// </summary>
        [Display(Name = "Publish on date/time (PST):")]
        [DataType(DataType.DateTime)]
        [DateTimeUtcKind]
        public virtual DateTimeOffset? Updated { get; set; }

        /// <summary>
        /// Content within the BODY tag
        /// </summary>
        [DataType(DataType.Html)]
        public string Content { get; set; }

        /// <summary>
        /// Content injected into the HEAD tag
        /// </summary>
        [DataType(DataType.Html)]
        public string HeadJavaScript { get; set; }

        /// <summary>
        /// Content injected just above the closing BODY tag below the Content inject.
        /// </summary>
        [DataType(DataType.Html)] public string FooterJavaScript { get; set; }

        /// <summary>
        /// Code editor mode
        /// </summary>
        public EditorMode EditorMode { get; set; }

        /// <summary>
        /// Content is valid and OK to save
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Editing field
        /// </summary>
        public string EditingField { get; set; }

        /// <summary>
        /// Editor title (different than Article title)
        /// </summary>
        public string EditorTitle { get; set; }

        /// <summary>
        /// URL Path
        /// </summary>
        public string UrlPath { get; set; }

        /// <summary>
        /// Array of editor fields
        /// </summary>
        public IEnumerable<EditorField> EditorFields { get; set; }

        /// <summary>
        /// Array of custom buttons for this editor
        /// </summary>
        public IEnumerable<string> CustomButtons { get; set; }

        /// <summary>
        /// Editor type
        /// </summary>
        public string EditorType { get; set; } = nameof(EditCodePostModel);

        /// <summary>
        /// Update existing version
        /// </summary>
        public bool UpdateExisting { get; set; } = true;

        /// <summary>
        /// Article Banner Image
        /// </summary>
        public string BannerImage { get; set; }
    }
}