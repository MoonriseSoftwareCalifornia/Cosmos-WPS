using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Cosmos.Cms.Models
{
    /// <summary>
    ///     Code editor return result model
    /// </summary>
    public class SaveCodeResultJsonModel
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SaveCodeResultJsonModel()
        {
            Errors = new List<ModelStateEntry>();
        }

        /// <summary>
        ///     Form post is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        ///     Error count
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        ///     Has stopped counting errors because it has reached its maximum
        /// </summary>
        public bool HasReachedMaxErrors { get; set; }

        /// <summary>
        ///     Model valiation state
        /// </summary>
        public ModelValidationState ValidationState { get; set; }

        /// <summary>
        ///     Errors in model state
        /// </summary>
        public List<ModelStateEntry> Errors { get; set; }

        /// <summary>
        ///   Updated model
        /// </summary>
        public EditCodePostModel Model { get; set; } = null;
    }
}