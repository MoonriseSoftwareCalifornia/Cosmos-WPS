using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Search and replace mode
    /// </summary>
    public class SearchAndReplaceViewModel
    {
        /// <summary>
        /// Include content in search and replace?
        /// </summary>
        [Display(Name = "Include content in search and replace?")]
        public bool IncludeContent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Include title in search and replace?")]
        public bool IncludeTitle { get; set; }

        /// <summary>
        /// Limit to article
        /// </summary>
        [Display(Name = "Limit to article:")]
        public int? ArticleNumber { get; set; }
        /// <summary>
        /// Limit to only published articles?
        /// </summary>
        [Display(Name = "Limit to published articles?")]
        public bool LimitToPublished { get; set; } = true;
        /// <summary>
        /// Find:
        /// </summary>
        [Display(Name = "Find:")]
        public string FindValue { get; set; }
        /// <summary>
        /// Replace with:
        /// </summary>
        [Display(Name = "Replace:")]
        public string ReplaceValue { get; set; }
    }
}
