using System.Collections.Generic;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Delete blob items
    /// </summary>
    public class DeleteBlobItemsViewModel
    {
        /// <summary>
        /// Parent path when delete was executed
        /// </summary>
        public string ParentPath { get; set; }
        /// <summary>
        /// Full paths of items to delete
        /// </summary>
        public List<string> Paths { get; set; }
    }
}
