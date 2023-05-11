using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Common.Data
{
    /// <summary>
    /// Setting
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// Setting ID
        /// </summary>
        [Key]
        [Display(Name = "Id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Setting group
        /// </summary>
        [Required]
        [MinLength(1)]
        [Display(Name = "Group")]
        public string Group { get; set; }

        /// <summary>
        /// Setting Name
        /// </summary>
        [Required]
        [MinLength(1)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Setting value
        /// </summary>
        [Required]
        [MinLength(1)]
        [Display(Name = "Value")]
        public string Value { get; set; }

        /// <summary>
        /// Setting value is required
        /// </summary>
        [Required]
        [Display(Name = "Is Required")]
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Description of setting
        /// </summary>
        [Required]
        [MinLength(1)]
        [Display(Name = "Description")]
        public string Description { get; set; }

    }
}
