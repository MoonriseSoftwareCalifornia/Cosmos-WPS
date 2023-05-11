using Cosmos.Common.Data.Logic;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Common.Data
{
    /// <summary>
    /// Node script
    /// </summary>
    public class NodeScript
    {
        /// <summary>
        /// Script Id
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Endpoint Name
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        /// Version number
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Published date and time
        /// </summary>
        public DateTimeOffset? Published { get; set; }

        /// <summary>
        /// Date and time updated
        /// </summary>
        public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Script expiration date and time
        /// </summary>
        public DateTimeOffset? Expires { get; set; }

        /// <summary>
        ///     Status of this script
        /// </summary>
        /// <remarks>See <see cref="StatusCodeEnum" /> enum for code numbers.</remarks>
        public int StatusCode { get; set; } = 0;

        /// <summary>
        /// Input variables
        /// </summary>
        public string[] InputVars { get; set; }

        /// <summary>
        /// Input configuration
        /// </summary>
        public string Config { get; set; }

        /// <summary>
        /// Node JavaScript code
        /// </summary>
        [Required]
        [MinLength(1)]
        public string Code { get; set; }

        /// <summary>
        /// Description of what this script does
        /// </summary>
        [Required]
        [MinLength(1)]
        public string Description { get; set; }

        /// <summary>
        /// Roles allowed to access this endpoint.
        /// </summary>
        public string[] Roles { get; set; } = null;
    }
}
