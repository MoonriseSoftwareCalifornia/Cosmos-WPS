namespace Cosmos.BlobService.Config
{
    /// <summary>
    ///     Google configuration
    /// </summary>
    public class GoogleStorageConfig
    {
        /// <summary>
        ///     Project Id
        /// </summary>
        public string GoogleProjectId { get; set; }

        /// <summary>
        ///     JSON authorization path
        /// </summary>
        public string GoogleJsonAuthPath { get; set; }

        /// <summary>
        ///     Bucket name
        /// </summary>
        public string GoogleBucketName { get; set; }
    }
}