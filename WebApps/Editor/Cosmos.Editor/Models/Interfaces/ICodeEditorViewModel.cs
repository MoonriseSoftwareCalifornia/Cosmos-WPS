using System.Collections.Generic;

namespace Cosmos.Cms.Models.Interfaces
{
    /// <summary>
    /// Code editor view model interface
    /// </summary>
    public interface ICodeEditorViewModel
    {
        /// <summary>
        /// Editing field
        /// </summary>
        public string EditingField { get; set; }
        /// <summary>
        /// Editor title
        /// </summary>
        public string EditorTitle { get; set; }
        /// <summary>
        /// Content is valid
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// Array of editor fields
        /// </summary>
        public IEnumerable<EditorField> EditorFields { get; set; }
        /// <summary>
        /// Array of custom buttons
        /// </summary>
        public IEnumerable<string> CustomButtons { get; set; }

        /// <summary>
        /// Type of editor
        /// </summary>
        public string EditorType { get; set; }
    }
}