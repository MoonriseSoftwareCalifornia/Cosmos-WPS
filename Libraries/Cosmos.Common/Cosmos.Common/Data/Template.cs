using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Common.Data
{
    /// <summary>
    ///     A page template
    /// </summary>
    public class Template
    {
        /// <summary>
        ///     Identity key for this entity
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Display(Name = "Layout ID")]
        public Guid? LayoutId { get; set; }

        [Display(Name = "Community Layout Id")]
        public string CommunityLayoutId { get; set; }

        /// <summary>
        ///     Friendly name or title of this page template
        /// </summary>
        [Display(Name = "Template Title")]
        [StringLength(128)]
        public string Title { get; set; }

        /// <summary>
        ///     Description or notes about how to use this template
        /// </summary>
        [Display(Name = "Description/Notes")]
        public string Description { get; set; }

        /// <summary>
        ///     The HTML content of this page template
        /// </summary>
        [Display(Name = "HTML Content")]
        [DataType(DataType.Html)]
        public string Content { get; set; }

    }
}