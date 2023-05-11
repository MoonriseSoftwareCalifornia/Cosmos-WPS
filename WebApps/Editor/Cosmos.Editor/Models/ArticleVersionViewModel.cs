using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Article version view model
    /// </summary>
    public class ArticleVersionViewModel
    {
        /// <summary>
        /// Article Item ID
        /// </summary>
        [Display(Name = "Id")]
        public Guid Id { get; set; }
        /// <summary>
        /// Published date and time
        /// </summary>
        [Display(Name = "Published")]
        public DateTimeOffset? Published { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        [Display(Name = "Title")]
        public string Title { get; set; }
        /// <summary>
        /// Last updated date and time
        /// </summary>
        [Display(Name = "Updated")]
        public DateTimeOffset Updated { get; set; }
        /// <summary>
        /// Version number
        /// </summary>
        [Display(Name = "No.")]
        public int VersionNumber { get; set; }
        /// <summary>
        /// Expires date and time
        /// </summary>
        [Display(Name = "Expires")]
        public DateTimeOffset? Expires { get; set; }
        /// <summary>
        /// Uses Live editor
        /// </summary>
        [Display(Name = "Uses Live editor")]
        public bool UsesHtmlEditor { get; set; }
    }
}