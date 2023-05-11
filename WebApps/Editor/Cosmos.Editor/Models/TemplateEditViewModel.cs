using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Edit template title and description
    /// </summary>
    public class TemplateEditViewModel
    {
        /// <summary>
        /// Template ID
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Template title
        /// </summary>
        [Display(Name = "Template Title")]
        [StringLength(128)]
        [Required(AllowEmptyStrings = false)]
        public string Title { get; set; }
        /// <summary>
        /// Template description
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Description/Notes")]
        public string Description { get; set; }
    }
}
