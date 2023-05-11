using Cosmos.Common.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Article Edit Menu Item
    /// </summary>
    public class ArticleEditMenuItem
    {
        /// <inheritdoc cref="Article.Id" />
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Article Number
        /// </summary>
        public int ArticleNumber { get; set; }

        /// <inheritdoc cref="Article.VersionNumber" />
        public int VersionNumber { get; set; }

        /// <inheritdoc cref="Article.Published" />
        public DateTimeOffset? Published { get; set; }

        /// <summary>
        /// Can use Live editor
        /// </summary>
        public bool UsesHtmlEditor { get; set; }
    }
}
