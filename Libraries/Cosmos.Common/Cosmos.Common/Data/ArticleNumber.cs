using System;

namespace Cosmos.Common.Data
{
    /// <summary>
    /// Tracks article numbers
    /// </summary>
    public class ArticleNumber
    {
        /// <summary>
        /// Record ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Date and time number set
        /// </summary>
        public DateTimeOffset SetDateTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// Last Article Number
        /// </summary>
        public int LastNumber { get; set; }
    }
}
