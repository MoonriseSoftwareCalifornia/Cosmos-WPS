using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Common.Services.Configurations
{
    /// <summary>
    ///     Editor URL information
    /// </summary>
    public class EditorUrl
    {
        /// <summary>
        ///     Cloud provider
        /// </summary>
        [Required]
        [UIHint("CloudProvider")]
        [Display(Name = "Cloud")]
        public string CloudName { get; set; }

        /// <summary>
        ///     Editor Url
        /// </summary>
        [Required]
        [Url]
        [Display(Name = "Url")]
        [RegularExpression(@"^(https://)", ErrorMessage = "Must start with https://")]
        public string Url { get; set; }
    }
}