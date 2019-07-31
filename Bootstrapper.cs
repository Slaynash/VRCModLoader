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

        public event Action Destroyed = delegate {};
        
        void Awake()
        {
            if (Environment.CommandLine.Contains("--verbose") || ModPrefs.GetBool("vrctools", "enabledebugconsole", false))
            {
				VRCModLogger.Init();
				VRCModLogger.Log("[VRCModLoader] Logger Initialised");
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
            Destroyed();
        }
    }
}