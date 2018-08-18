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
                var bootstrapper = new GameObject("Bootstrapper").AddComponent<Bootstrapper>();
                bootstrapper.Destroyed += Bootstrapper_Destroyed;
            }
        }

        private static void Bootstrapper_Destroyed()
        {
            ModComponent.Create();
        }
    }
}
