using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRCModLoader
{
    public class ModComponent : MonoBehaviour
    {
        private CompositeModCaller mods;
        private bool freshlyLoaded = false;
        private bool quitting = false;

        internal static GameObject modulesGameObject;

        public static ModComponent Instance { get; private set; }

        public static void Create()
        {
            VRCModLogger.Log("[ModComponent] Loading VRLoader.dll");
            LoadVRLoader();
            VRCModLogger.Log("[ModComponent] VRLoader.dll loaded");

            if (!Bootstrapper.loadmods)
                return;

            try
            {
                VRCModLogger.Log("[ModComponent] Creating components");

                //First create the mod manager GO, so it gets updated before the modules.
                GameObject modManagerGO = new GameObject("IPA_ModManager");
                modulesGameObject = new GameObject("IPA_VRModules");
                modulesGameObject.SetActive(false); // We will enable it when the Ui scene will be loaded.
                modManagerGO.AddComponent<ModComponent>();
            }
            catch(Exception e)
            {
                VRCModLogger.LogError("[ModComponent] Error while creating instance: " + e);
            }
        }

        void Awake()
        {
            VRCModLogger.Log("[ModComponent] Awake called");
            DontDestroyOnLoad(gameObject);
            Instance = this;

            try
            {
                ModManager.LoadMods();
            }
            catch (Exception e)
            {
                VRCModLogger.Log("An error occured while loading mods: " + e);
            }

            mods = new CompositeModCaller(ModManager.ModControllers);
            mods.OnApplicationStart();

            SceneManager.sceneLoaded += (scene, method) =>
            {
                VRCModLogger.Log("[ModComponent] Scene Loaded: " + scene.name);

                if (scene.name == "ui")
                    StartCoroutine(StartVRModules());

                mods.OnLevelWasLoaded(scene.buildIndex);
            };
        }

        private IEnumerator StartVRModules()
        {
            yield return null; // Wait for end UI init
            modulesGameObject.SetActive(true);
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
            OnLevelWasLoadedNow(Application.loadedLevel);
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

        void OnLevelWasLoadedNow(int level)
        {
            VRCModLogger.Log("[ModComponent] OnLevelWasLoaded called (" + level + ")");
            transform.SetAsLastSibling();
            if (mods != null) mods.OnLevelWasLoaded(level);
            freshlyLoaded = true;
        }

        public static void OnModSettingsApplied()
        {
            Instance?.mods?.OnModSettingsApplied();
        }

    }
}
