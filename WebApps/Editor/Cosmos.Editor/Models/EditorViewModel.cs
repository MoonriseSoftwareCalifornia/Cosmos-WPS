namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Editor view model
    /// </summary>
    public class EditorViewModel
    {
        /// <summary>
        /// File name
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// HTML content
        /// </summary>
        public string Html { get; set; }
        /// <summary>
        /// Edit mode
        /// </summary>
        public bool EditModeOn { get; set; }
    }

}