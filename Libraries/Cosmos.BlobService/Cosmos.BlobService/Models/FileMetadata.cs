using System;

namespace Cosmos.BlobService.Models
{
    public class FileMetadata
    {
        /// <summary>
        /// Full file name including folder path for blob
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Mime type
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// Total number of bytes
        /// </summary>
        public long ContentLength { get; set; }
        /// <summary>
        /// ETag
        /// </summary>
        public string ETag { get; set; }
        /// <summary>
        /// Last modified
        /// </summary>
        public DateTimeOffset LastModified { get; set; }
        /// <summary>
        /// Upon upload, the UTC date time for the upload is saved as a 'tick'
        /// </summary>
        /// <remarks>
        /// This is used when blobs are being synchronized between storage accounts.
        /// </remarks>
        public long? UploadDateTime { get; set; }

        /// <summary>
        /// Unique ID of the upload
        /// </summary>
        /// <remarks>
        /// This is used when blobs are being synchronized between storage accounts.
        /// </remarks>
        public string UploadUid { get; set; }

        /// <summary>
        /// Size if file when uploaded.
        /// </summary>
        /// <remarks>
        /// This is used when blobs are being synchronized between storage accounts.
        /// </remarks>
        public long? UploadSize { get; set; }
    }
}
