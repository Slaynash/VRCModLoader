using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if (PC)
using Windows;
#endif

namespace VRCModLoader
{
    class Bootstrapper : MonoBehaviour
    {
        internal static bool loadmods = true;

        void Awake()
        {
            VRCModLogger.Init();
            VRCModLogger.Log("[VRCModLoader] Logger Initialised");
#if (PC)
            if ((Environment.CommandLine.Contains("--verbose") || ModPrefs.GetBool("vrctools", "enabledebugconsole", false)))
            {
                VRCModLogger.consoleEnabled = true;
                GuiConsole.CreateConsole();
                VRCModLogger.Log("[VRCModLoader] Bootstrapper created");
            }

            if (Environment.CommandLine.Contains("--nomodloader"))
            {
                loadmods = false;
            }
#endif
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
