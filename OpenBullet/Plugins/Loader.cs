using OpenBullet.Views.UserControls;
using PluginFramework;
using RuriLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenBullet.Plugins
{
    public static class Loader
    {
        /// <summary>
        /// Loads the plugins in a folder one by one.
        /// </summary>
        /// <param name="folder">The folder where plugins are located</param>
        /// <returns>A tuple with the collection of plugin controls and block plugin controls.</returns>
        public static (IEnumerable<PluginControl>, IEnumerable<IBlockPlugin>) LoadPlugins(string folder)
        {
            var plugins = new List<PluginControl>();
            var blockPlugins = new List<IBlockPlugin>();

            foreach (var dll in Directory.GetFiles(folder, "*.dll"))
            {
                var asm = Assembly.LoadFrom(dll);

                // Hook the dependency folder (a folder with the name of the DLL) if it exists
                var depFolder = Path.Combine(OB.pluginsFolder, Path.GetFileNameWithoutExtension(dll));
                if (Directory.Exists(depFolder))
                {
                    Hook(new string[] { depFolder });
                }

                // Load the dependencies
                LoadDependencies(asm.GetReferencedAssemblies());

                // Search all the types in the assembly
                foreach (var type in asm.GetTypes())
                {
                    // If it implements the IPlugin interface
                    if (type.GetInterface(nameof(IPlugin)) == typeof(IPlugin))
                    {
                        plugins.Add(new PluginControl(type, OB.App));
                    }
                    // If it implements the IBlockPlugin interface and derives from BlockBase
                    else if (type.GetInterface(nameof(IBlockPlugin)) == typeof(IBlockPlugin) 
                        && type.GetTypeInfo().IsSubclassOf(typeof(BlockBase)))
                    {
                        blockPlugins.Add(Activator.CreateInstance(type) as IBlockPlugin);
                    }
                }
            }

            return (plugins, blockPlugins);
        }

        /// <summary>
        /// Loads dependencies recursively in a greedy fashion.
        /// </summary>
        /// <param name="assemblies">The assemblies for which to load dependencies.</param>
        public static void LoadDependencies(IEnumerable<AssemblyName> assemblies)
        {
            foreach (var asm in assemblies)
            {
                // Make sure we didn't load it yet
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().FullName == asm.FullName))
                {
                    try
                    {
                        AppDomain.CurrentDomain.Load(asm);

                        // Load more dependencies recursively
                        LoadDependencies(Assembly.Load(asm).GetReferencedAssemblies());
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Hooks folders to the AssemblyResolve of the current AppDomain.
        /// </summary>
        /// <param name="folders">The folders where to search for assemblies</param>
        // Found on https://stackoverflow.com/questions/33975073/proper-way-to-resolving-assemblies-from-subfolders
        public static void Hook(params string[] folders)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                // Check if the requested assembly is part of the loaded assemblies
                var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
                if (loadedAssembly != null)
                    return loadedAssembly;

                // This resolver is called when an loaded control tries to load a generated XmlSerializer - We need to discard it.
                // http://connect.microsoft.com/VisualStudio/feedback/details/88566/bindingfailure-an-assembly-failed-to-load-while-using-xmlserialization

                var n = new AssemblyName(args.Name);

                if (n.Name.EndsWith(".xmlserializers", StringComparison.OrdinalIgnoreCase))
                    return null;

                // http://stackoverflow.com/questions/4368201/appdomain-currentdomain-assemblyresolve-asking-for-a-appname-resources-assembl

                if (n.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
                    return null;

                string assy = null;

                // Find the corresponding assembly file
                foreach (var dir in folders)
                {
                    assy = new[] { "*.dll", "*.exe" }.SelectMany(g => Directory.EnumerateFiles(dir, g)).FirstOrDefault(f =>
                    {
                        try { return n.Name.Equals(AssemblyName.GetAssemblyName(f).Name, StringComparison.OrdinalIgnoreCase); }
                        catch (BadImageFormatException) { return false; /* Bypass assembly is not a .net exe */ }
                        catch (Exception ex) { throw new ApplicationException("Error loading assembly " + f, ex); }
                    });

                    if (assy != null)
                        return Assembly.LoadFrom(assy);
                }

                throw new ApplicationException("Assembly " + args.Name + " not found");
            };
        }
    }
}
