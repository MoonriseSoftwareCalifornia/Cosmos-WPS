using Cosmos.Common.Models;
using Cosmos.Cms.Models.Interfaces;
using System.Collections.Generic;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Compare code between two pages
    /// </summary>
    public class CompareCodeViewModel : ICodeEditorViewModel
    {
        /// <summary>
        /// Editing field
        /// </summary>
        public string EditingField { get; set; }

        /// <summary>
        /// Editor title (different than Article title)
        /// </summary>
        public string EditorTitle { get; set; }

        /// <summary>
        /// Content is valid and OK to save
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// Array of editor fields
        /// </summary>
        public IEnumerable<EditorField> EditorFields { get; set; }

        /// <summary>
        /// Array of custom buttons for this editor
        /// </summary>
        public IEnumerable<string> CustomButtons { get; set; }

        /// <summary>
        /// Editor type
        /// </summary>
        public string EditorType { get; set; } = nameof(CompareCodeViewModel);

        /// <summary>
        /// Articles to compare
        /// </summary>
        public ArticleViewModel[] Articles { get; set; }
    }
}
