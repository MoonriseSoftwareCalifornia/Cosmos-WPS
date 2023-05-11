using System;

namespace Cosmos.BlobService
{
    /// <summary>
    /// File manager entry
    /// </summary>
    public class FileManagerEntry
    {
        /// <summary>
        /// Date created
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// Date created in UTC
        /// </summary>
        public DateTime CreatedUtc { get; set; }
        /// <summary>
        /// File extention
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// If folder, does it have child folders?
        /// </summary>
        public bool HasDirectories { get; set; }
        /// <summary>
        /// Is a directory?
        /// </summary>
        public bool IsDirectory { get; set; }
        /// <summary>
        /// Date/time modified
        /// </summary>
        public DateTime Modified { get; set; }
        /// <summary>
        /// Date/time modified in UTC
        /// </summary>
        public DateTime ModifiedUtc { get; set; }
        /// <summary>
        /// Item name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Item path
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Size in bytes
        /// </summary>
        public long Size { get; set; }
    }
}