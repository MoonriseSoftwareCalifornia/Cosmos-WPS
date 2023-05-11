using System.ComponentModel.DataAnnotations;

namespace Cosmos.BlobService.Config
{
    /// <summary>
    ///     Cosmos configuration model
    /// </summary>
    public class CosmosStorageConfig
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public CosmosStorageConfig()
        {
            StorageConfig = new StorageConfig();
        }

        /// <summary>
        ///     Primary cloud for this installation.
        /// </summary>
        [Display(Name = "Primary Cloud")]
        [UIHint("CloudProvider")]
        public string PrimaryCloud { get; set; }

        /// <summary>
        ///     Blob service configuration
        /// </summary>
        public StorageConfig StorageConfig { get; set; }

    }
}