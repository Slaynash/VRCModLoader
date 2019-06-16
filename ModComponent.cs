using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace VRCModLoader
{
    public class ModComponent : MonoBehaviour
    {
        private CompositeModCaller mods;
        private bool freshlyLoaded = false;
        private bool quitting = false;

        public static ModComponent Instance { get; private set; }

        public static ModComponent Create()
        {
            VRCModLogger.Log("[ModComponent] Loading VRLoader.dll");
            LoadVRLoader();
            VRCModLogger.Log("[ModComponent] VRLoader.dll loaded");

            try
            {
                VRCModLogger.Log("[ModComponent] Creating component");
                return new GameObject("IPA_ModManager").AddComponent<ModComponent>();
            }
            catch(Exception e)
            {
                VRCModLogger.LogError("[ModComponent] Error while creating instance: " + e);
                return null;
            }
        }

        void Awake()
        {
            VRCModLogger.Log("[ModComponent] Awake called");
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

        private static void LoadVRLoader()
        {
            try
            {
                Assembly.Load(Properties.Resources.VRLoader);
            }
            catch (Exception e)
            {
                VRCModLogger.LogError("[ModComponent] Error while loading VRLoader.dll: " + e);
            }
        }

        void Start()
        {
            VRCModLogger.Log("[ModComponent] Start called");
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
            VRCModLogger.Log("[ModComponent] Component destroyed");
            if (!quitting)
            {
                Create();
            }
        }
        
        void OnApplicationQuit()
        {
            VRCModLogger.Log("[ModComponent] OnApplicationQuit called");
            if (mods != null) mods.OnApplicationQuit();

            quitting = true;
        }

        void OnLevelWasLoaded(int level)
        {
            VRCModLogger.Log("[ModComponent] OnLevelWasLoaded called (" + level + ")");
            transform.SetAsLastSibling();
            if (level == 0) StartCoroutine(VRCToolsUpdater.UpdateAndRebootIfRequired());
            if (mods != null) mods.OnLevelWasLoaded(level);
            freshlyLoaded = true;
        }

    }
}
