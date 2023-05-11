using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Template index view model
    /// </summary>
    public class TemplateIndexViewModel
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        [Key] public Guid Id { get; set; }
        /// <summary>
        /// Name of the associated layout
        /// </summary>
        [Display(Name = "Associated Layout")]
        public string LayoutName { get; set; }

        /// <summary>
        /// Template title
        /// </summary>
        [Display(Name = "Template Title")]
        [StringLength(128)]
        [Required(AllowEmptyStrings = false)]
        public string Title { get; set; }
        /// <summary>
        /// Description/Notes regarding this template
        /// </summary>
        [Display(Name = "Description/Notes")] public string Description { get; set; }

        /// <summary>
        /// Template can use live editor
        /// </summary>
        public bool UsesHtmlEditor { get; set; }
    }
}