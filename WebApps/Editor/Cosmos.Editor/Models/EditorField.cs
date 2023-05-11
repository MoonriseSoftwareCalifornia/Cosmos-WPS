namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Editor field metadata
    /// </summary>
    public class EditorField
    {
        /// <summary>
        /// Field ID
        /// </summary>
        public string FieldId { get; set; }
        /// <summary>
        /// Field Name
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// Editor mode
        /// </summary>
        public EditorMode EditorMode { get; set; }

        /// <summary>
        /// Icon URL
        /// </summary>
        public string IconUrl { get; set; } = "";

        /// <summary>
        /// Tool tip content.
        /// </summary>
        public string ToolTip { get; set; } = string.Empty;
    }

    /// <summary>
    /// Monaco Editor Mode
    /// </summary>
    public enum EditorMode
    {
        /// <summary>
        /// JavaScript mode
        /// </summary>
        JavaScript = 0,
        /// <summary>
        /// HTML Mode
        /// </summary>
        Html = 1,
        /// <summary>
        /// CSS Mode
        /// </summary>
        Css = 2,
        /// <summary>
        /// XML Mode
        /// </summary>
        Xml = 3,
        /// <summary>
        /// JSON
        /// </summary>
        Json = 4
    }
}