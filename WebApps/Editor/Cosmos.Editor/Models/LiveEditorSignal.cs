using System;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Live editor SignalR model
    /// </summary>
    public class LiveEditorSignal
    {
        /// <summary>
        /// Id of the Article entity being worked on.
        /// </summary>
        public Guid ArticleId { get; set; }

        /// <summary>
        /// Edit ID as defined by the data-ccms-ceid attribute.
        /// </summary>
        public string EditorId { get; set; }

        /// <summary>
        /// User Id (Email address)
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// User position in document
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Command
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Is Focused
        /// </summary>
        public bool IsFocused { get; set; }

        /// <summary>
        /// HTML data being sent back
        /// </summary>
        public object Data { get; set; }
    }
}
