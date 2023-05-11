using Cosmos.Cms.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Layout code view model
    /// </summary>
    public class LayoutCodeViewModel : ICodeEditorViewModel
    {
        /// <summary>
        /// Layout ID number
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// HEAD content of the layout
        /// </summary>
        public string Head { get; set; }
        /// <summary>
        /// HTML Header content
        /// </summary>
        /// <remarks>Often is either a NAV or HEADER tag content, or both.</remarks>
        public string HtmlHeader { get; set; }

        /// <summary>
        /// Body tag HTML attributes
        /// </summary>
        public string BodyHtmlAttributes { get; set; }
        /// <summary>
        /// Layout footer.
        /// </summary>
        public string FooterHtmlContent { get; set; }
        /// <summary>
        /// Current edit field.
        /// </summary>
        public string EditingField { get; set; }
        /// <summary>
        /// Editor title
        /// </summary>
        public string EditorTitle { get; set; }
        /// <summary>
        /// List of editor fields
        /// </summary>
        public IEnumerable<EditorField> EditorFields { get; set; }
        /// <summary>
        /// Custom button list.
        /// </summary>
        public IEnumerable<string> CustomButtons { get; set; }
        /// <summary>
        /// Model is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Editor type
        /// </summary>
        public string EditorType { get; set; } = nameof(LayoutCodeViewModel);
    }
}