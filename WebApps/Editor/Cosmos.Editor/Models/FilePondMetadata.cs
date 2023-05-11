using Newtonsoft.Json;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Filepond upload metadata
    /// </summary>
    public class FilePondMetadata
    {
        /// <summary>
        /// Upload path or folder
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }
        /// <summary>
        /// subfolder (of appplicable) of upload
        /// </summary>
        [JsonProperty("relativePath")]
        public string RelativePath { get; set; }

        /// <summary>
        /// Name of file being uploaded
        /// </summary>
        [JsonProperty("fileName")]
        public string FileName { get; set; }
    }
}
