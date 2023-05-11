using Cosmos.Common.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    ///     Article version list info item
    /// </summary>
    [Serializable]
    public class ArticleVersionInfo
    {
        /// <inheritdoc cref="Article.Id" />
        [Key]
        public Guid Id { get; set; }

        /// <inheritdoc cref="Article.VersionNumber" />
        public int VersionNumber { get; set; }

        /// <inheritdoc cref="Article.Title" />
        public string Title { get; set; }

        /// <inheritdoc cref="Article.Updated" />
        public DateTimeOffset Updated { get; set; }

        /// <inheritdoc cref="Article.Published" />
        public DateTimeOffset? Published { get; set; }

        /// <inheritdoc cref="Article.Expires" />
        public DateTimeOffset? Expires { get; set; }

        /// <summary>
        /// Can use Live editor
        /// </summary>
        public bool UsesHtmlEditor { get; set; }
    }
}