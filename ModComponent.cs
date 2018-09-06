using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRCModLoader
{
    internal class ModComponent : MonoBehaviour
    {
        private CompositeModCaller mods;
        private bool freshlyLoaded = false;
        private bool quitting = false;

        public static ModComponent Instance { get; private set; }

        public static ModComponent Create()
        {
            VRCModLogger.Log("[VRCMod] [ModComponent] Creating component");
            return new GameObject("IPA_ModManager").AddComponent<ModComponent>();
        }

        void Awake()
        {
            VRCModLogger.Log("[VRCMod] [ModComponent] Awake called");
            DontDestroyOnLoad(gameObject);
            Instance = this;

            if (VRCToolsUpdater.CheckForVRCToolsUpdate())
            {
                VRCToolsUpdater.SheduleVRCToolsUpdate();
            }
            else
            {
                mods = new CompositeModCaller(ModManager.ModControllers);
                mods.OnApplicationStart();
            }
        }

        void Start()
        {
            VRCModLogger.Log("[VRCMod] [ModComponent] Start called");
            OnLevelWasLoaded(Application.loadedLevel);
        }

        void Update()
        {
            if (freshlyLoaded)
            {
                freshlyLoaded = false;
                if (mods != null) mods.OnLevelWasInitialized(Application.loadedLevel);
            }
            if (mods != null) mods.OnUpdate();
        }

        void LateUpdate()
        {
            if(mods != null) mods.OnLateUpdate();
        }

        void FixedUpdate()
        {
            if (mods != null) mods.OnFixedUpdate();
        }

        void OnGUI()
        {
            if (mods != null) mods.OnGUI();
        }

        void OnDestroy()
        {
            VRCModLogger.Log("[VRCMod] [ModComponent] Component destroyed");
            if (!quitting)
            {
                Create();
            }
        }
        
        void OnApplicationQuit()
        {
            VRCModLogger.Log("[VRCMod] [ModComponent] OnApplicationQuit called");
            if (mods != null) mods.OnApplicationQuit();

            quitting = true;
        }

        void OnLevelWasLoaded(int level)
        {
            VRCModLogger.Log("[VRCMod] [ModComponent] OnLevelWasLoaded called (" + level + ")");
            if (level == 0) StartCoroutine(VRCToolsUpdater.UpdateAndRebootIfRequired());
            if (mods != null) mods.OnLevelWasLoaded(level);
            freshlyLoaded = true;
        }

    }
}
