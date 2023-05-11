using System.Collections.Generic;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Files to move plus the path to move files to
    /// </summary>
    public class MoveFilesViewModel
    {
        /// <summary>
        /// The folder where items will be placed
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Items to be moved.
        /// </summary>
        public List<string> Items { get; set; }
    }
}
