using System;
using Windows;
using UnityEngine;

namespace VRCModLoader
{
    class Bootstrapper : MonoBehaviour
    {
        internal static bool loadmods = true;
        
        void Awake()
        {
            VRCModLogger.Init();
            VRCModLogger.Log("[VRCModLoader] Logger Initialised");

            if (Environment.CommandLine.Contains("--verbose") || ModPrefs.GetBool("vrctools", "enabledebugconsole", false))
            {
                VRCModLogger.consoleEnabled = true;
                GuiConsole.CreateConsole();
                VRCModLogger.Log("[VRCModLoader] Bootstrapper created");
            }

            if (Environment.CommandLine.Contains("--nomodloader"))
            {
                loadmods = false;
            }
            if (!Environment.CommandLine.Contains("--notitle"))
            {
                var windowPtr = GuiConsole.FindWindow(null, "VRChat");
                GuiConsole.SetWindowText(windowPtr, GuiConsole.Title);
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
    }
}
