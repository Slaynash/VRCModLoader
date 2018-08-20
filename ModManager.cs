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

namespace VRCModLoader
{
    public static class ModManager
    {
        private static List<VRCMod> _Mods = null;
        private static List<VRCModController> _ModControllers = null;

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


        public static IEnumerable<VRCMod> Mods
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


        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return ModComponent.Instance.StartCoroutine(routine);
        }




        private static void LoadMods()
        {
            string tmpmodDirectory = Path.Combine(Environment.CurrentDirectory, "Mods_tmp");
            string modDirectory = Path.Combine(Environment.CurrentDirectory, "Mods");

            if (Directory.Exists(tmpmodDirectory)) Directory.Delete(tmpmodDirectory, true); // delete the temp directory if existing

            // Process.GetCurrentProcess().MainModule crashes the game and Assembly.GetEntryAssembly() is NULL,
            // so we need to resort to P/Invoke
            string exeName = Path.GetFileNameWithoutExtension(AppInfo.StartupPath);
            VRCModLogger.Log(exeName);
            _Mods = new List<VRCMod>();
            _ModControllers = new List<VRCModController>();

            if (!Directory.Exists(modDirectory)) return;
            Directory.CreateDirectory(tmpmodDirectory);

            String[] files = Directory.GetFiles(modDirectory, "*.dll");
            foreach (var s in files)
            {
                string newPath = tmpmodDirectory + s.Substring(modDirectory.Length);
                VRCModLogger.Log("Copying " + s + " to " + newPath);
                File.Copy(s, newPath);
                LoadModsFromFile(newPath, exeName);
            }
            

            // DEBUG
            VRCModLogger.Log("Running on Unity " +UnityEngine.Application.unityVersion);
            VRCModLogger.Log("-----------------------------");
            VRCModLogger.Log("Loading mods from " + tmpmodDirectory + " and found " + _Mods.Count);
            VRCModLogger.Log("-----------------------------");
            foreach (var mod in _Mods)
            {
                VRCModLogger.Log(" " + mod.Name + " (" + mod.Version + ") by " + mod.Author + (mod.DownloadLink != null ? " (" + mod.DownloadLink + ")" : ""));
            }
            VRCModLogger.Log("-----------------------------");
        }

        private static void LoadModsFromFile(string file, string exeName)
        {
            List<VRCMod> mods = new List<VRCMod>();

            if (!File.Exists(file) || !file.EndsWith(".dll", true, null))
                return;

            try
            {
                Assembly assembly = Assembly.LoadFrom(file);

                foreach (Type t in assembly.GetTypes())
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
                            VRCModLogger.Log("[WARN] Could not load mod " + t.FullName + " in " + Path.GetFileName(file) + "! " + e);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                VRCModLogger.Log("[ERROR] Could not load " + Path.GetFileName(file) + "! " + e);
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
