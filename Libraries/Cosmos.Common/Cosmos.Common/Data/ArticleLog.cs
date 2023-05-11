using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Common.Data
{
    /// <summary>
    ///     Article activity log entry
    /// </summary>
    public class ArticleLog
    {
        /// <summary>
        ///     Identity key of the entity
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        ///     User ID of the person who triggered the activity
        /// </summary>

        public string IdentityUserId { get; set; }

        /// <summary>
        ///     ID of the Article associated with this event.
        /// </summary>
        public Guid ArticleId { get; set; }

        /// <summary>
        /// Title of the article
        /// </summary>
        public string ArticleTitle { get; set; }

        /// <summary>
        ///     Notes regarding what happened.
        /// </summary>
        public string ActivityNotes { get; set; }

        /// <summary>
        ///     Date and Time (UTC by default)
        /// </summary>
        public DateTimeOffset DateTimeStamp { get; set; } = DateTimeOffset.UtcNow;

        #region NAVIGATION


        #endregion
    }
}