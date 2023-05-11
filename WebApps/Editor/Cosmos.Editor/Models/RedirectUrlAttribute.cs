﻿using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Validates if a URL is a valid redirect URL
    /// </summary>
    public class RedirectUrlAttribute : ValidationAttribute
    {
        /// <summary>
        ///     Determines if value is valid.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var url = (string)value;

            if (string.IsNullOrEmpty(url))
            {
                return new ValidationResult("Url is required.");
            }

            if (!url.StartsWith("/") && !url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                return new ValidationResult("Url must start with '/' or 'https://' or 'http://'.");
            }

            return ValidationResult.Success;
        }
    }
}
