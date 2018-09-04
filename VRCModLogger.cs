using System;
using UnityEngine;

namespace VRCModLoader
{
    public class VRCModLogger
    {
        public static void Log(string s)
        {
            Console.WriteLine("[VRCMod] " + s);
            Debug.Log("[VRCMod] " + s);
        }

        public static void Log(string s, params object[] args)
        {
            Console.WriteLine("[VRCMod] " + s, args);
            Debug.LogFormat("[VRCMod] " + s, args);
        }

        public static void LogError(string s)
        {
            Console.WriteLine("[VRCMod] [Error] " + s);
            Debug.Log("[VRCMod] [Error] " + s);
        }

        public static void LogError(string s, params object[] args)
        {
            Console.WriteLine("[VRCMod] [Error] " + s, args);
            Debug.LogFormat("[VRCMod] [Error] " + s, args);
        }
    }
}