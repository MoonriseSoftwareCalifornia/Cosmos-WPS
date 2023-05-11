using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Layout index view model
    /// </summary>
    public class LayoutIndexViewModel
    {
        /// <summary>
        /// Layout ID
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Is the default layout
        /// </summary>
        [Display(Name = "Default website layout?")]
        public bool IsDefault { get; set; } = false;
        /// <summary>
        /// Layout name
        /// </summary>
        [Display(Name = "Layout Name")]
        [StringLength(128)]
        public string LayoutName { get; set; }
        /// <summary>
        /// Layout notes
        /// </summary>
        [Display(Name = "Notes")]
        [DataType(DataType.Html)]
        public string Notes { get; set; }
    }
}