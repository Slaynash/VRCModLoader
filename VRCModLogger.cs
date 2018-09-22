using System;
using UnityEngine;

namespace VRCModLoader
{
    public class VRCModLogger
    {
        internal static bool consoleEnabled = false;

        public static void Log(string s)
        {
            if(consoleEnabled) Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] [VRCMod] " + s);
            Debug.Log("[VRCMod] " + s);
        }

        public static void Log(string s, params object[] args)
        {
            if (consoleEnabled) Console.WriteLine("[VRCMod] " + s, args);
            Debug.LogFormat("[VRCMod] " + s, args);
        }

        public static void LogError(string s)
        {
            if (consoleEnabled) Console.WriteLine("[VRCMod] [Error] " + s);
            Debug.Log("[VRCMod] [Error] " + s);
        }

        public static void LogError(string s, params object[] args)
        {
            if (consoleEnabled) Console.WriteLine("[VRCMod] [Error] " + s, args);
            Debug.LogFormat("[VRCMod] [Error] " + s, args);
        }
    }
}