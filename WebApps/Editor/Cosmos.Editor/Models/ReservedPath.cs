using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Reserved path
    /// </summary>
    /// <remarks>
    /// A reserved path prevents a page from being named that conflicts with a path.
    /// </remarks>
    public class ReservedPath
    {
        /// <summary>
        /// Row ID
        /// </summary>
        [Key]
        [Display(Name = "Id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Reserved Path
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Reserved Path")]
        public string Path { get; set; }
        /// <summary>
        /// Is required by Cosmos
        /// </summary>
        [Display(Name = "Required by Cosmos?")]
        public bool CosmosRequired { get; set; } = false;
        /// <summary>
        /// Reason by protected
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }
}
