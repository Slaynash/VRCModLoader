using System;
using System.IO;
using UnityEngine;

namespace VRCModLoader
{
    public static class Injector
    {
        private static bool injected = false;
        public static void Inject()
        {
            if (!injected)
            {
                injected = true;
               new GameObject("Bootstrapper").AddComponent<Bootstrapper>();
            }
        }
    }
}
