using System.Collections.Generic;

namespace Cosmos.BlobService.Config
{
    /// <summary>
    ///     Storage provider configuration
    /// </summary>
    public class StorageConfig
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public StorageConfig()
        {
            AmazonConfigs = new List<AmazonStorageConfig>();
            AzureConfigs = new List<AzureStorageConfig>();
            GoogleConfigs = new List<GoogleStorageConfig>();
        }

        /// <summary>
        ///     Amazon configuration
        /// </summary>
        public List<AmazonStorageConfig> AmazonConfigs { get; set; }

        /// <summary>
        ///     Azure configuration
        /// </summary>
        public List<AzureStorageConfig> AzureConfigs { get; set; }

        /// <summary>
        ///     Google configuration
        /// </summary>
        public List<GoogleStorageConfig> GoogleConfigs { get; set; }
    }
}