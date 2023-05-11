namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Filerobot image post model
    /// </summary>
    public class FileRobotImagePost
    {
        /// <summary>
        /// File name without extension
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// File name with extension
        /// </summary>
        public string fullName { get; set; }
        /// <summary>
        /// File extension
        /// </summary>
        public string extension { get; set; }
        /// <summary>
        /// Mime type
        /// </summary>
        public string mimeType { get; set; }
        /// <summary>
        /// Base 64 image data
        /// </summary>
        public string imageBase64 { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        public double? quantity { get; set; } = null;
        /// <summary>
        /// Image width
        /// </summary>
        public int width { get; set; }
        /// <summary>
        /// Image height
        /// </summary>
        public int height { get; set; }
        /// <summary>
        /// Folder where image should reside
        /// </summary>
        public string folder { get; set; }
    }
}
