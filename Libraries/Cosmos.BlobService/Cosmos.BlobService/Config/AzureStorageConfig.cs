﻿using System.ComponentModel.DataAnnotations;

namespace Cosmos.BlobService.Config
{
    /// <summary>
    ///     Azure storage config
    /// </summary>
    public class AzureStorageConfig
    {
        /// <summary>
        ///     Connection string
        /// </summary>
        [Display(Name = "Conn. String")]
        public string AzureBlobStorageConnectionString { get; set; }

        /// <summary>
        ///     Container name
        /// </summary>
        [Display(Name = "Container")]
        public string AzureBlobStorageContainerName { get; set; } = "$web";

        /// <summary>
        ///     Storage end point
        /// </summary>
        [Display(Name = "Website URL")]
        public string AzureBlobStorageEndPoint { get; set; }

        [Display(Name = "File share")]
        public string AzureFileShare { get; set; }
    }
}