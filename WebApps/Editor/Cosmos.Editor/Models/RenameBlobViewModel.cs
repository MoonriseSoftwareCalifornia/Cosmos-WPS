namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Rename blob item view model
    /// </summary>
    public class RenameBlobViewModel
    {
        /// <summary>
        /// From blob name
        /// </summary>
        public string FromBlobName { get; set; }

        /// <summary>
        /// Rename to blob name
        /// </summary>
        public string ToBlobName { get; set; }

        /// <summary>
        /// The folder where the blob is located
        /// </summary>
        public string BlobRootPath { get; set; }

        /// <summary>
        /// Item being renamed is a directory
        /// </summary>
        public bool IsDirectory { get; set; }
    }
}
