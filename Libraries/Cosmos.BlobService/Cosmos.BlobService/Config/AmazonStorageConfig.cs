using System.ComponentModel.DataAnnotations;

namespace Cosmos.BlobService.Config
{
    /// <summary>
    ///     Amazon S3 configuration
    /// </summary>
    public class AmazonStorageConfig
    {
        /// <summary>
        ///     Access key Id
        /// </summary>
        [Display(Name = "Key Id")]
        public string AmazonAwsAccessKeyId { get; set; }

        /// <summary>
        ///     AWS secret access key
        /// </summary>
        [Display(Name = "Key")]
        public string AmazonAwsSecretAccessKey { get; set; }

        /// <summary>
        ///     Amazon bucket name
        /// </summary>
        [Display(Name = "Bucket")]
        public string AmazonBucketName { get; set; }

        /// <summary>
        ///     Amazon region
        /// </summary>
        [Display(Name = "Region")]
        [UIHint("AmazonRegions")]
        public string AmazonRegion { get; set; }

        /// <summary>
        ///     Service URL
        /// </summary>
        [Display(Name = "Website URL")]
        public string ServiceUrl { get; set; }

        /// <summary>
        ///     Profile name
        /// </summary>
        [Display(Name = "Conn. Name")]
        public string ProfileName { get; set; }
    }
}