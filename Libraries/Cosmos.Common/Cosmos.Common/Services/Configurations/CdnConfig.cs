using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Common.Services.Configurations
{
    /// <summary>
    ///     CDN Configurations
    /// </summary>
    public class CdnConfig
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public CdnConfig()
        {
            AzureCdnConfig = new AzureCdnConfig();
        }

        /// <summary>
        ///     Number of seconds to cache data (20 minutes default).
        /// </summary>
        [Display(Name = "Cache duration (seconds)")]
        public int CacheDuration { get; set; } = 1200;

        /// <summary>
        ///     Azure CDN Configuration
        /// </summary>
        public AzureCdnConfig AzureCdnConfig { get; set; }
    }
}