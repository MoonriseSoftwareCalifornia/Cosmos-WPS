namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Parameters used to identity an Azure CDN endpoint
    /// </summary>
    public class AzureCdnEndpoint
    {
        /// <summary>
        /// End point unique ID
        /// </summary>
        public string EndPointId { get; set; }
        /// <summary>
        /// Subscription ID
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Name of resource group where CDN is located
        /// </summary>
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Name of CDN Profile
        /// </summary>
        public string CdnProfileName { get; set; }

        /// <summary>
        /// Unique endpoint name
        /// </summary>
        public string EndpointName { get; set; }

        /// <summary>
        /// CDN Type (or SKU)
        /// </summary>
        public string SkuName { get; set; }

        /// <summary>
        /// End point host name
        /// </summary>
        public string EndPointHostName { get; set; }
    }
}
