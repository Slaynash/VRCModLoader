using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Windows;

namespace VRCModLoader
{
    class Bootstrapper : MonoBehaviour
    {
        internal static bool loadmods = true;

        void Awake()
        {
            VRCModLogger.Init();
            VRCModLogger.Log("[VRCModLoader] Logger Initialised");

            if ((Environment.CommandLine.Contains("--verbose") || ModPrefs.GetBool("vrctools", "enabledebugconsole", false)) && Application.platform == RuntimePlatform.WindowsPlayer)
            {
                VRCModLogger.consoleEnabled = true;
                GuiConsole.CreateConsole();
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
    }
}
