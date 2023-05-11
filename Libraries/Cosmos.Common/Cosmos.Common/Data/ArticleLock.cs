using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Common.Data
{
    /// <summary>
    /// Represents a lock on an article because it is being edited.
    /// </summary>
    public class ArticleLock
    {
        /// <summary>
        /// Unique ID for this record
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Unique SignalR Connection Id
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// User ID for this lock
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// Article RECORD ID for this lock (Not the Article ID)
        /// </summary>
        public Guid ArticleId { get; set; }

        /// <summary>
        /// When the lock was set
        /// </summary>
        public DateTimeOffset LockSetDateTime { get; set; }

        /// <summary>
        /// Editor type for this lock
        /// </summary>
        public string EditorType { get; set; }

        /// <summary>
        /// File path for this lock (if applicable)
        /// </summary>
        public string FilePath { get; set; }
    }
}
