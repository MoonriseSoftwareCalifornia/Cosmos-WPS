namespace Cosmos.Cms.Models
{
    /// <summary>
    ///     JSON model returned have Live editor saves content
    /// </summary>
    public class SaveResultJsonModel : SaveCodeResultJsonModel
    {
        /// <summary>
        /// Server indicates the content was saved successfully in the database
        /// </summary>
        public bool ServerSideSuccess { get; set; }

        /// <summary>
        ///     Content model as saved
        /// </summary>
        public new HtmlEditorViewModel Model { get; set; }
    }
}