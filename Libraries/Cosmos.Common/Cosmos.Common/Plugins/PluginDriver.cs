using Cosmos.Common.Plugins.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Cosmos.Common.Plugins
{
    /// <summary>
    /// Gets plugins for execution
    /// </summary>
    /// <remarks><see href="https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support"/></remarks>
    public class PluginDriver
    {

        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly string _sharePath;
        private readonly bool _isEditor;

        /// <summary>
        /// Gets the root path of the plugins
        /// </summary>
        /// <returns></returns>
        private string GetPluginRootPath()
        {
            return Path.Combine(_sharePath, "Plugins");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostEnvironment"></param>
        /// <param name="sharePath"></param>
        /// <param name="isEditor"></param>
        public PluginDriver(IWebHostEnvironment hostEnvironment, string sharePath, bool isEditor)
        {
            _hostEnvironment = hostEnvironment;
            _sharePath = sharePath;
            _isEditor = isEditor;
        }

        /// <summary>
        /// Gets all the plugins loaded on this instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>
        /// For example either <typeparamref name="Cosmos.Cms"/> or Cosmos.Cms.Publisher
        /// </remarks>
        public IEnumerable<IPlugin> GetPlugins<T>()
        {
            var pluginPaths = Directory.GetDirectories(GetPluginRootPath());

            IEnumerable<IPlugin> plugins = pluginPaths.SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = LoadPlugin<T>(pluginPath);
                return GetPluginsFromAssembly(pluginAssembly);
            }).ToList();

            return plugins;
        }


        #region PRIVATE METHODS

        /// <summary>
        /// Loads a plugin for either publisher or editor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        /// <remarks>
        /// For example either <typeparamref name="Cosmos.Cms"/> or Cosmos.Cms.Publisher
        /// </remarks>
        private Assembly LoadPlugin<T>(string relativePath)
        {
            // Navigate up to the solution root
            string root = GetPluginRootPath();

            string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
            Console.WriteLine($"Loading commands from: {pluginLocation}");
            Loader loadContext = new Loader(pluginLocation);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        }

        /// <summary>
        /// Gets all the plugins for an assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static IEnumerable<IPlugin> GetPluginsFromAssembly(Assembly assembly)
        {
            int count = 0;
            foreach (var type in assembly.GetTypes().Where(type => typeof(IPlugin).IsAssignableFrom(type)))
            {
                IPlugin result = Activator.CreateInstance(type) as IPlugin;
                if (result != null)
                {
                    count++;
                    yield return result;
                }
            }
        }

        #endregion
    }
}
