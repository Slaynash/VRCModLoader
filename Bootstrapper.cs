using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace VRCModLoader
{
    class Bootstrapper : MonoBehaviour
    {
        internal static bool loadmods = true;
        internal static MethodInfo CreateConsoleMethod;
        
        void Awake()
        {
            VRCModLogger.Init();
            VRCModLogger.Log("[VRCModLoader] Logger Initialised");

            if (Environment.CommandLine.Contains("--verbose") || ModPrefs.GetBool("vrctools", "enabledebugconsole", false))
            {
                VRCModLogger.consoleEnabled = true;
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                    CreateConsole();
                VRCModLogger.Log("[VRCModLoader] Bootstrapper created");
            }

            if (Environment.CommandLine.Contains("--nomodloader"))
            {
                loadmods = false;
            }
        }

        void Start()
        {
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            try
            {
                ModComponent.Create();
            }
            catch (Exception e)
            {
                VRCModLogger.LogError(e.ToString());
            }
        }

        private static void CreateConsole()
        {
            if (CreateConsoleMethod == null)
            {
                Type foundType = null;
                bool foundconsoletype = AppDomain.CurrentDomain.GetAssemblies().Any(a =>
                {
                    foundType = a.GetType("Windows.GuiConsole");
                    return (foundType != null);
                });
                if (foundType != null)
                    CreateConsoleMethod = foundType.GetMethod("CreateConsole");
            }
            if (CreateConsoleMethod != null)
                CreateConsoleMethod.Invoke(null, null);
        }
    }
}
