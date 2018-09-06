using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRCModLoader
{
    internal class CompositeModCaller
    {
        IEnumerable<VRCModController> modControllers;

        private delegate void CompositeCall(VRCModController modController);

        public CompositeModCaller(IEnumerable<VRCModController> modControllers)
        {
            this.modControllers = modControllers;  
        }

        public void OnApplicationStart()
        {
            Invoke(modController => modController.OnApplicationStart());
        }

        public void OnApplicationQuit()
        {
            Invoke(modController => modController.OnApplicationQuit());
        }

        public void OnLevelWasLoaded(int level)
        {
            foreach (var modController in modControllers)
            {
                try
                {
                    modController.OnLevelWasLoaded(level);
                }
                catch (Exception ex)
                {
                    VRCModLogger.LogError("{0}: {1}", modController.mod.Name, ex);
                }
            }
        }


        private void Invoke(CompositeCall callback)
        {
            foreach (var modController in modControllers)
            {
                try
                {
                    callback(modController);
                }
                catch (Exception ex)
                {
                    VRCModLogger.LogError("{0}: {1}", modController.mod.Name, ex);
                }
            }
        }



        public void OnLevelWasInitialized(int level)
        {
            foreach (var modController in modControllers)
            {
                try
                {
                    modController.OnLevelWasInitialized(level);
                }
                catch (Exception ex)
                {
                    VRCModLogger.LogError("{0}: {1}", modController.mod.Name, ex);
                }
            }
        }


        public void OnUpdate()
        {
            Invoke(modController => modController.OnUpdate());
        }

        public void OnFixedUpdate()
        {
            Invoke(modController => modController.OnFixedUpdate());
        }

        public void OnLateUpdate()
        {
            Invoke(modController => modController.OnLateUpdate());
        }
        public void OnGUI()
        {
            Invoke(modController => modController.OnGUI());
        }
    }
}
