using System;
using System.Collections.Generic;

namespace Cosmos.Common.Plugins.Interfaces
{
    /// <summary>
    /// Interface for Cosmos Plugins
    /// </summary>
    public interface IPlugin
    {
        #region PROPERTIES
        /// <summary>
        /// A unique GUID that identifies this plugin
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Author of plugin
        /// </summary>
        string Author { get; }
        /// <summary>
        /// Author website
        /// </summary>
        Uri AuthorUrl { get; }
        /// <summary>
        /// A JSON string that describes the fields in the configuration interface
        /// </summary>
        string ConfigJson { get; }
        /// <summary>
        /// Description of what the plugin does
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Indicates if this plugin is only for the editor web app
        /// </summary>
        bool EditorOnly { get; }
        /// <summary>
        /// Indicates if a menu pick should be provided in the editor menu
        /// </summary>
        bool EditorMenu { get; }
        /// <summary>
        /// Name of the plugin
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Version of plugin
        /// </summary>
        string Version { get; }
        /// <summary>
        /// Paths to templates
        /// </summary>
        IEnumerable<string> TemplatePaths { get; }

        #endregion

        #region METHODS

        /// <summary>
        /// Configuration that gets loaded from settings
        /// </summary>
        /// <param name="config"></param>
        void Config(string config);

        /// <summary>
        /// Execution method
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        string Execute(string[] args);

        #endregion
    }
}
