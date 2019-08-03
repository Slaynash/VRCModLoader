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
        internal static ModuleManager moduleManager;

        private static List<VRCMod> _Mods = null;
        private static List<VRCModController> _ModControllers = null;
        private static List<VRModule> _Modules = null;
        private static List<Assembly> loadedAssemblies = new List<Assembly>();

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
                    LoadMods();
                return _Mods;
            }
        }
        
        public static List<VRModule> Modules
        {
            get
            {
                if (_Modules == null)
                    LoadMods();
                return _Modules;
            }
        }
        

        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return ModComponent.Instance.StartCoroutine(routine);
        }




        private static void LoadMods()
        {
            VRCModLogger.Log("Looking for mods");
            string modDirectory = Path.Combine(Environment.CurrentDirectory, "Mods");

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

            foreach (string s in files)
            {
                if (!File.Exists(s) || !s.EndsWith(".dll", true, null))
                    return;

                VRCModLogger.Log("Loading " + s);
                try
                {
                    byte[] data = File.ReadAllBytes(s);
                    Assembly a = Assembly.Load(data);
                    loadedAssemblies.Add(a);
                }
                catch (Exception e)
                {
                    VRCModLogger.LogError("Unable to load assembly " + s + ":\n" + e);
                }
            }

            foreach (Assembly a in loadedAssemblies)
            {
                VRCModLogger.Log("Loading mods from " + a.GetName());
                LoadModsFromAssembly(a);
            }


            // DEBUG
            VRCModLogger.Log("Running on Unity " + Application.unityVersion);
            VRCModLogger.Log("-----------------------------");
            VRCModLogger.Log("Loading mods from " + modDirectory + " and found " + _Mods.Count + " VRCMods and " + Modules.Count + " VRModules.");
            VRCModLogger.Log("-----------------------------");
            foreach (var mod in _Mods)
                VRCModLogger.Log(" " + mod.Name + " (" + mod.Version + ") by " + mod.Author + (mod.DownloadLink != null ? " (" + mod.DownloadLink + ")" : ""));

            VRCModLogger.Log("-----------------------------");

            foreach (var mod in _Modules)
                VRCModLogger.Log(" " + mod.Name + " (" + mod.Version + ") by " + mod.Author);
            
            VRCModLogger.Log("-----------------------------");
        }

        private static void LoadModsFromAssembly(Assembly assembly)
        {
            try
            {
                foreach (Type t in assembly.GetLoadableTypes())
                {
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
                            VRCModLogger.Log("[WARN] [ModManager] Could not load mod " + t.FullName + " in " + assembly.GetName() + "! " + e);
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
                            VRCModLogger.Log("[WARN] [ModManager] Could not load module " + t.FullName + " in " + assembly.GetName() + "! " + e);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                VRCModLogger.LogError("[ModManager] Could not load " + assembly.GetName() + "! " + e);
            }
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
