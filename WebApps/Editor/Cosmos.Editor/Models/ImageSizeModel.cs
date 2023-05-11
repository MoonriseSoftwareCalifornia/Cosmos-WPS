namespace Cosmos.Cms.Models
{
    /// <summary>
    ///     Image size mode used for thumbnail generator
    /// </summary>
    public class ImageSizeModel
    {
        /// <summary>
        ///     Width in pixels
        /// </summary>
        public int Width { get; set; } = 80;

        /// <summary>
        ///     Height in pixels
        /// </summary>
        public int Height { get; set; } = 80;
    }
}