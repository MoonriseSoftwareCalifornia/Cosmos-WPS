namespace Cosmos.Cms.Models
{
    /// <summary>
    /// New folder view model
    /// </summary>
    public class NewFolderViewModel
    {
        /// <summary>
        /// The parent folder where new folder is created as a child
        /// </summary>
        public string ParentFolder { get; set; }

        /// <summary>
        /// New folder name
        /// </summary>
        public string FolderName { get; set; }

        /// <summary>
        /// Directory only mode for file browser
        /// </summary>
        public bool DirectoryOnly { get; set; }
        /// <summary>
        /// Directory only mode for file browser
        /// </summary>
        public string Container { get; set; } = "$web";
    }
}
