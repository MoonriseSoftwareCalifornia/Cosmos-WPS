using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Common.Services.Configurations
{
    /// <summary>
    ///     Azure blob storage config
    /// </summary>
    public class BlobServiceConfig
    {
        /// <summary>
        ///     Id of the provider
        /// </summary>
        [Key]
        [Display(Name = "Id")]
        public int Id { get; set; }

        /// <summary>
        ///     Cloud provider
        /// </summary>
        [Required]
        [UIHint("CloudProvider")]
        [Display(Name = "Cloud Provider")]
        public string CloudName { get; set; }

        /// <summary>
        ///     Is primary storage for this website
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        ///     Blob storage connection string.
        /// </summary>
        [Required]
        [Display(Name = "Blob storage connection string")]
        public string ConnectionString { get; set; }
    }
}