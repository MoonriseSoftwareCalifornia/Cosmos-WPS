using Cosmos.Cms.Common.Services.Configurations.Storage;
using System.Collections.Generic;

namespace Cosmos.Cms.Common.Services.Configurations
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
            AzureConfigs = new List<AzureStorageConfig>();
        }

        /// <summary>
        ///     Azure configuration
        /// </summary>
        public List<AzureStorageConfig> AzureConfigs { get; set; }
    }
}