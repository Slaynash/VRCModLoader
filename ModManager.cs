using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using VRLoader.Attributes;
using VRLoader.Modules;

namespace VRCModLoader
{
    public static class ModManager
    {
        private static Dictionary<string, string> _Names = new Dictionary<string, string>();
        private static List<string> _Loaded = new List<string>();
        private static List<VRCMod> _Mods = null;
        private static List<VRCModController> _ModControllers = null;
        private static List<VRModule> _Modules = null;
        internal static ModuleManager moduleManager;

        /// <summary>
        /// Gets the list of loaded mods and loads them if necessary.
        /// </summary>
        internal static IEnumerable<VRCModController> ModControllers
        {
            get
            {
                if(_ModControllers == null)
                {
                    try
                    {
                        LoadMods();
                    }
                    catch(Exception e)
                    {
                        VRCModLogger.Log("An error occured while loading mods: " + e);
                    }
                }
                return _ModControllers;
            }
        }

        public static List<VRCMod> Mods
        {
            get
            {
                if (_Mods == null)
                {
                    LoadMods();
                }
                return _Mods;
            }
        }
        
        public static List<VRModule> Modules
        {
            get
            {
                if (_Modules == null)
                {
                    LoadMods();
                }
                return _Modules;
            }
        }
        

        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return ModComponent.Instance.StartCoroutine(routine);
        }




        private static void LoadMods()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            VRCModLogger.Log("Looking for mods");
            string modDirectory = Path.Combine(Environment.CurrentDirectory, "Mods");

            // Process.GetCurrentProcess().MainModule crashes the game and Assembly.GetEntryAssembly() is NULL,
            // so we need to resort to P/Invoke
            string exeName = Path.GetFileNameWithoutExtension(AppInfo.StartupPath);
            VRCModLogger.Log(exeName);
            _Mods = new List<VRCMod>();
            _ModControllers = new List<VRCModController>();
            _Modules = new List<VRModule>();
            if (moduleManager == null)
            {
                moduleManager = new ModuleManager();
                ModComponent.Instance.gameObject.AddComponent<VRLoader.VRLoader>();
            }
            if (!Directory.Exists(modDirectory)) return;

            string[] files = Directory.GetFiles(modDirectory, "*.dll");
            foreach (var s in files)
                LoadAssemblyNameFromFile(s);
            foreach (var s in files)
                LoadModsFromFile(s);


            // DEBUG
            VRCModLogger.Log("Running on Unity " +UnityEngine.Application.unityVersion);
            VRCModLogger.Log("-----------------------------");
            VRCModLogger.Log("Loading mods from " + modDirectory + " and found " + _Mods.Count + " mods and " + Modules.Count + " modules.");
            VRCModLogger.Log("-----------------------------");
            foreach (var mod in _Mods)
            {
                VRCModLogger.Log(" " + mod.Name + " (" + mod.Version + ") by " + mod.Author + (mod.DownloadLink != null ? " (" + mod.DownloadLink + ")" : ""));
            }
            
            foreach (var mod in _Modules)
            {
                VRCModLogger.Log(" " + mod.Name + " (" + mod.Version + ") by " + mod.Author);
            }
            
            VRCModLogger.Log("-----------------------------");
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (_Names.ContainsKey(args.Name))
                return LoadModsFromFile(_Names[args.Name]);
            return null;
        }

        private static void LoadAssemblyNameFromFile(string file)
        {
            if (!File.Exists(file) || !file.EndsWith(".dll", true, null) || _Loaded.Contains(file))
                return;

            try
            {
                AssemblyName name = AssemblyName.GetAssemblyName(file);

                _Names.Add(name.FullName, file);
            }
            catch (Exception ex)
            {
                VRCModLogger.LogError("[ModManager] Could not get assembly name of " + Path.GetFileName(file) + "! " + ex);
            }
        }

        private static Assembly LoadModsFromFile(string file)
        {

            if (!File.Exists(file) || !file.EndsWith(".dll", true, null) || _Loaded.Contains(file))
                return null;

            try
            {
                Assembly assembly = Assembly.Load(File.ReadAllBytes(file));
                VRCModLogger.Log("File: " + file);
                foreach (Type t in assembly.GetLoadableTypes())
                {
                    //VRCModLogger.Log("Type: " + t.FullName + " - (VRCMod: " + t.IsSubclassOf(typeof(VRCMod)) + " - VRModule: " + t.IsSubclassOf(typeof(VRModule)) + ")");
                    if (t.IsSubclassOf(typeof(VRCMod)))
                    {
                        try
                        {
                            VRCMod modInstance = Activator.CreateInstance(t) as VRCMod;
                            _Mods.Add(modInstance);
                            _ModControllers.Add(new VRCModController(modInstance));
                            VRCModInfoAttribute modInfoAttribute = modInstance.GetType().GetCustomAttributes(typeof(VRCModInfoAttribute), true).FirstOrDefault() as VRCModInfoAttribute;
                            if (modInfoAttribute != null)
                            {
                                modInstance.Name = modInfoAttribute.Name;
                                modInstance.Version = modInfoAttribute.Version;
                                modInstance.Author = modInfoAttribute.Author;
                                modInstance.DownloadLink = modInfoAttribute.DownloadLink;
                            }
                        }
                        catch (Exception e)
                        {
                            VRCModLogger.Log("[WARN] [ModManager] Could not load mod " + t.FullName + " in " + Path.GetFileName(file) + "! " + e);
                        }
                    }
                    
                    if (t.IsSubclassOf(typeof(VRModule)))
                    {
                        try
                        {
                            ModuleInfoAttribute moduleInfo;
                            if ((moduleInfo = (t.GetCustomAttributes(typeof(ModuleInfoAttribute), true).FirstOrDefault<object>() as ModuleInfoAttribute)) != null)
                            {
                                VRModule vrmodule = ModComponent.Instance.gameObject.AddComponent(t) as VRModule;
                                _Modules.Add(vrmodule);
                                vrmodule.Initialize(moduleInfo, moduleManager);
                                VRCModLogger.Log("[VRLoader] {0} loaded.", vrmodule);
                            }
                        }
                        catch (Exception e)
                        {
                            VRCModLogger.Log("[WARN] [ModManager] Could not load module " + t.FullName + " in " + Path.GetFileName(file) + "! " + e);
                        }
                    }
                }
                _Loaded.Add(file);
                return assembly;
            }
            catch (Exception e)
            {
                VRCModLogger.LogError("[ModManager] Could not load " + Path.GetFileName(file) + "! " + e);
            }
            return null;
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                VRCModLogger.LogError("[ModManager] An error occured while getting types from assembly " + assembly.GetName().Name + ". Returning types from error.\n" + e);
                return e.Types.Where(t => t != null);
            }
        }

        public class AppInfo
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = false)]
            private static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
            private static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
            public static string StartupPath
            {
                get
                {
                    StringBuilder stringBuilder = new StringBuilder(260);
                    GetModuleFileName(NullHandleRef, stringBuilder, stringBuilder.Capacity);
                    return stringBuilder.ToString();
                }
            }
        }

    }
}
