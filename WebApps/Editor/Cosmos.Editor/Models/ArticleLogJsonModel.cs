using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Article log json model
    /// </summary>
    public class ArticleLogJsonModel
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key] public Guid Id { get; set; }

        /// <summary>
        /// Activity notes and description
        /// </summary>
        public string ActivityNotes { get; set; }

        /// <summary>
        ///     Date and Time (UTC by default)
        /// </summary>
        public DateTimeOffset DateTimeStamp { get; set; }
        /// <summary>
        /// Identity User Id
        /// </summary>
        public string IdentityUserId { get; set; }
    }
}