using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Common.Services.Configurations
{
    /// <summary>
    ///     Configuration for Azure CDN
    /// </summary>
    public class AzureCdnConfig
    {
        /// <summary>
        ///     Client Id
        /// </summary>
        [Display(Name = "Client Id")]
        public string ClientId { get; set; }

        /// <summary>
        ///     CDN Provider Name
        /// </summary>
        [UIHint("AzureCdnProviders")]
        [Display(Name = "CDN Provider Name")]
        public string CdnProvider { get; set; }

        /// <summary>
        ///     Client Secret
        /// </summary>
        [Display(Name = "Client Secret")]
        public string ClientSecret { get; set; }

        /// <summary>
        ///     Tenant Id
        /// </summary>
        [Display(Name = "Tenant Id")]
        public string TenantId { get; set; }

        /// <summary>
        ///     Tenant Domain Name
        /// </summary>
        [Display(Name = "Tenant Domain Name")]
        public string TenantDomainName { get; set; }

        /// <summary>
        ///     CDN Profile Name
        /// </summary>
        [Display(Name = "CDN Profile Name")]
        public string CdnProfileName { get; set; }

        /// <summary>
        ///     End Point Name
        /// </summary>
        [Display(Name = "End Point Name")]
        public string EndPointName { get; set; }

        /// <summary>
        ///     Azure Resource Group
        /// </summary>
        [Display(Name = "Azure Resource Group")]
        public string ResourceGroup { get; set; }

        /// <summary>
        ///     Subscription Id
        /// </summary>
        [Display(Name = "Subscription Id")]
        public string SubscriptionId { get; set; }
    }
}