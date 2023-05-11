using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Common.Models
{
    /// <summary>
    ///     Ensures that a DateTime object is of kind UTC
    /// </summary>
    public class DateTimeUtcKindAttribute : ValidationAttribute
    {
        /// <summary>
        ///     Determines if value is valid.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var t = value.GetType();

            if (t == typeof(DateTimeOffset))
            {
                return ValidationResult.Success;
            }

            if (t == typeof(DateTime?) || t == typeof(DateTime))
            {
                var dateTime = (DateTime?)value;

                if (dateTime.HasValue && dateTime.Value.Kind != DateTimeKind.Utc)
                    return new ValidationResult($"Must be DateTimeKind.Utc, not {dateTime.Value.Kind}.");
            }

            return ValidationResult.Success;
        }
    }
}