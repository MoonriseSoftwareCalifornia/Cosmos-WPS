namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Website preload options
    /// </summary>
    public class PreloadViewModel
    {
        /// <summary>
        /// Preload CDN
        /// </summary>
        public bool PreloadCdn { get; set; } = true;
        /// <summary>
        /// Redis objects created
        /// </summary>
        public int? PageCount { get; set; }
        /// <summary>
        /// Number of editors involved with preload operation
        /// </summary>
        public int EditorCount { get; set; } = 0;
    }
}
