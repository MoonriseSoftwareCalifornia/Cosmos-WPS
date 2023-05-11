using Cosmos.Cms.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Edit script post back model
    /// </summary>
    public class EditScriptPostModel : ICodeEditorViewModel
    {
        /// <summary>
        /// Article ID
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Endpoint Name
        /// </summary>
        [Required]
        [MaxLength(60)]
        [MinLength(1)]
        [RegularExpression("^[\\w-_.]*$", ErrorMessage = "Letters and numbers only.")]
        public string EndPoint { get; set; }

        /// <summary>
        /// Roles that can execute this script (if applicable)
        /// </summary>
        public string RoleList { get; set; }

        /// <summary>
        /// Title of endpoint
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Version number
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Input variables
        /// </summary>
        [RegularExpression("^[a-zA-Z0-9,.-:]*$", ErrorMessage = "Letters and numbers only.")]
        public string InputVars { get; set; }

        /// <summary>
        /// Input configuration
        /// </summary>
        public string Config { get; set; }

        /// <summary>
        /// Node JavaScript code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Published date and time
        /// </summary>
        public DateTimeOffset? Published { get; set; }

        /// <summary>
        /// Editing field
        /// </summary>
        public string EditingField { get; set; }

        /// <summary>
        /// Editor title
        /// </summary>
        public string EditorTitle { get; set; }

        /// <summary>
        /// Code is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Editor fields
        /// </summary>
        public IEnumerable<EditorField> EditorFields { get; set; }

        /// <summary>
        /// Custom buttons
        /// </summary>
        public IEnumerable<string> CustomButtons { get; set; }

        /// <summary>
        /// Editor type
        /// </summary>
        public string EditorType { get; set; } = nameof(EditScriptPostModel);

        /// <summary>
        /// Description of what this script does.
        /// </summary>
        [Required]
        [MinLength(2)]
        public string Description { get; set; }

        /// <summary>
        /// Indicates if this is to be saved as a new version.
        /// </summary>
        public bool? SaveAsNewVersion { get; set; }
    }
}
