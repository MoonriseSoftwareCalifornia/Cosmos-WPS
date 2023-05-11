using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Cosmos.Common.Plugins
{
    /// <summary>
    /// Loads an individual plugin
    /// </summary>
    /// <remarks><see href="https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support"/></remarks>
    internal class Loader : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public Loader(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
