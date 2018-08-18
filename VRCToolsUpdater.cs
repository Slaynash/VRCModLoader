using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;

namespace VRCModLoader
{
    internal class VRCToolsUpdater
    {
        private static bool needUpdate = false;
        private static Image downloadProgressFillImage = null;
        private static string vrctoolsPath = null;

        private static bool errored = false;
        private static int errorCode = -1;

        private static VRCUiManager uiManagerInstance;
        private static VRCUiPopupManager uiPopupManagerInstance;

        internal static bool CheckForVRCToolsUpdate()
        {
            vrctoolsPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))) + "\\Mods\\VRCTools.dll";
            Directory.CreateDirectory(Path.GetDirectoryName(vrctoolsPath));
            VRCModLogger.Log("[VRCToolsUpdater] Supposed VRCToolsPath path: " + vrctoolsPath);

            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (arg.ToLower().Equals("--vrctools.forceupdate")) return true;
                if (arg.ToLower().Equals("--vrctools.noupdate")) return false;
            }

            //hash check
            string fileHash = "";

            if (File.Exists(vrctoolsPath))
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(vrctoolsPath))
                    {
                        var hash = md5.ComputeHash(stream);
                        fileHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
                VRCModLogger.Log("[VRCToolsUpdater] Local VRCToolsPath file hash: " + fileHash);

                WWW hashCheckWWW = new WWW("https://vrchat.survival-machines.fr/vrcmod/VRCToolsHashCheck.php?localhash=" + fileHash);
                while (!hashCheckWWW.isDone) ;
                int responseCode = getResponseCode(hashCheckWWW);
                VRCModLogger.Log("[VRCToolsUpdater] hash check webpage returned [" + responseCode + "] \"" + hashCheckWWW.text + "\"");
                if (responseCode != 200) {
                    errored = true;
                    errorCode = responseCode;
                    return true;
                }
                else if(hashCheckWWW.text.Equals("OUTOFDATE"))
                {
                    VRCModLogger.Log("[VRCToolsUpdater] Update of VRCTools available");
                    return true;
                }
            }
            else
            {
                VRCModLogger.Log("[VRCToolsUpdater] Download of VRCTools required");
                return true;
            }

            return false;
            
        }

        internal static void SheduleVRCToolsUpdate()
        {
            if (!errored)
            {
                VRCModLogger.Log("[VRCToolsUpdater] Sheduling update");
                needUpdate = true;
            }
        }

        internal static IEnumerator UpdateAndRebootIfRequired()
        {
            if (needUpdate || errored)
            {
                bool goForUpdate = needUpdate;
                needUpdate = false;
                VRCModLogger.Log("[VRCToolsUpdater] Looking for VRCFlowManager");
                VRCFlowManager[] flowManagers = Resources.FindObjectsOfTypeAll<VRCFlowManager>();
                foreach(VRCFlowManager flowManager in flowManagers)
                {
                    flowManager.enabled = false;
                }
                VRCModLogger.Log("[VRCToolsUpdater] Disabled " + flowManagers.Length + " VRCFlowManager");


                if (GameObject.Find("UserInterface") == null)
                {
                    VRCModLogger.Log("[VRCToolsUpdater] Loading additive scene \"ui\"");
                    AssetManagement.LoadLevelAdditive("ui");
                }

                if (goForUpdate)
                {
                    needUpdate = false;

                    bool choiceDone = false;
                    bool update = false;
                    yield return ShowPopup("VRCTools Updater", "A VRCTools update is available", "Update", () => {
                        choiceDone = true;
                        update = true;
                    }, "Ignore", () => {
                        choiceDone = true;
                    });
                    yield return new WaitUntil(() => choiceDone);

                    if (update)
                    {
                        yield return ShowUpdatePopup();

                        VRCModLogger.Log("[VRCToolsUpdater] Update popup shown");

                        WWW vrctoolsDownload = new WWW("https://vrchat.survival-machines.fr/vrcmod/VRCTools.dll");
                        yield return vrctoolsDownload;
                        while (!vrctoolsDownload.isDone)
                        {
                            VRCModLogger.Log("[VRCToolsUpdater] Download progress: " + vrctoolsDownload.progress);
                            downloadProgressFillImage.fillAmount = vrctoolsDownload.progress;
                            yield return null;
                        }

                        int responseCode = getResponseCode(vrctoolsDownload);
                        VRCModLogger.Log("[VRCToolsUpdater] Download done ! response code: " + responseCode);
                        VRCModLogger.Log("[VRCToolsUpdater] File size: " + vrctoolsDownload.bytes.Length);

                        if (responseCode == 200)
                        {
                            yield return ShowPopup("VRCTools Updater", "Saving VRCTools");
                            VRCModLogger.Log("[VRCToolsUpdater] Saving file");
                            File.WriteAllBytes(vrctoolsPath, vrctoolsDownload.bytes);

                            VRCModLogger.Log("[VRCToolsUpdater] Showing restart dialog");
                            choiceDone = false;
                            yield return ShowPopup("VRCTools Updater", "Update downloaded", "Restart", () => {
                                choiceDone = true;
                            });
                            yield return new WaitUntil(() => choiceDone);

                            yield return ShowPopup("VRCTools Updater", "Restarting game");
                            VRCModLogger.Log("[VRCToolsUpdater] Rebooting game");
                            string args = "";
                            foreach (string arg in Environment.GetCommandLineArgs())
                            {
                                args = args + " ";
                            }

                            Thread t = new Thread(() =>
                            {
                                Thread.Sleep(1000);
                                System.Diagnostics.Process.Start(Path.GetDirectoryName(Path.GetDirectoryName(vrctoolsPath)) + "\\VRChat.exe", args);
                                Thread.Sleep(100);
                            });
                            t.Start();

                            Application.Quit();
                        }
                        else
                        {
                            yield return ShowErrorPopup("Unable to update VRCTools: Server returned code " + responseCode);
                        }
                    }
                    else
                    {
                        uiPopupManagerInstance.HideCurrentPopup();
                        foreach (VRCFlowManager flowManager in flowManagers)
                        {
                            flowManager.enabled = true;
                        }
                        VRCModLogger.Log("[VRCToolsUpdater] Enabled " + flowManagers.Length + " VRCFlowManager");
                    }
                    
                }
                else if (errored)
                {

                    yield return ShowErrorPopup("Unable to check VRCTools validity: Server returned code " + errorCode);

                }
            }
        }






        
        private static IEnumerator WaitForUIManager()
        {
            VRCModLogger.Log("WaitForUIManager");
            if (uiManagerInstance == null)
            {
                FieldInfo[] nonpublicStaticFields = typeof(VRCUiManager).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
                if (nonpublicStaticFields.Length == 0)
                {
                    VRCModLogger.Log("[VRCToolsUpdater] nonpublicStaticFields.Length == 0");
                    yield break;
                }
                FieldInfo uiManagerInstanceField = nonpublicStaticFields.First(field => field.FieldType == typeof(VRCUiManager));
                if (uiManagerInstanceField == null)
                {
                    VRCModLogger.Log("[VRCToolsUpdater] uiManagerInstanceField == null");
                    yield break;
                }
                uiManagerInstance = uiManagerInstanceField.GetValue(null) as VRCUiManager;
                VRCModLogger.Log("[VRCToolsUpdater] Waiting for UI Manager...");
                while (uiManagerInstance == null)
                {
                    uiManagerInstance = uiManagerInstanceField.GetValue(null) as VRCUiManager;
                    yield return null;
                }
                VRCModLogger.Log("[VRCToolsUpdater] UI Manager loaded");
            }
        }

        private static IEnumerator ShowPopup(string title, string body, string middleButton, Action middleButtonAction, Action<VRCUiPopup> additionnalSetup = null)
        {
            VRCModLogger.Log("ShowPopup");
            yield return WaitForUIManager();
            if (uiPopupManagerInstance == null)
            {
                FieldInfo[] nonpublicStaticPopupFields = typeof(VRCUiPopupManager).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
                if (nonpublicStaticPopupFields.Length == 0)
                {
                    VRCModLogger.Log("[VRCToolsUpdater] nonpublicStaticPopupFields.Length == 0");
                    yield break;
                }
                FieldInfo uiPopupManagerInstanceField = nonpublicStaticPopupFields.First(field => field.FieldType == typeof(VRCUiPopupManager));
                if (uiPopupManagerInstanceField == null)
                {
                    VRCModLogger.Log("[VRCToolsUpdater] uiPopupManagerInstanceField == null");
                    yield break;
                }
                uiPopupManagerInstance = uiPopupManagerInstanceField.GetValue(null) as VRCUiPopupManager;
            }

            if (uiPopupManagerInstance == null)
            {
                VRCModLogger.Log("[VRCToolsUpdater] uiPopupManagerInstance == null");
                yield break;
            }

            uiPopupManagerInstance.ShowStandardPopup(title, body, middleButton, middleButtonAction, additionnalSetup);
        }

        private static IEnumerator ShowPopup(string title, string body, string leftButton, Action leftButtonAction, string rightButton, Action rightButtonAction, Action<VRCUiPopup> additionnalSetup = null)
        {
            VRCModLogger.Log("ShowPopup");
            yield return WaitForUIManager();
            if (uiPopupManagerInstance == null)
            {
                FieldInfo[] nonpublicStaticPopupFields = typeof(VRCUiPopupManager).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
                if (nonpublicStaticPopupFields.Length == 0)
                {
                    VRCModLogger.Log("[VRCToolsUpdater] nonpublicStaticPopupFields.Length == 0");
                    yield break;
                }
                FieldInfo uiPopupManagerInstanceField = nonpublicStaticPopupFields.First(field => field.FieldType == typeof(VRCUiPopupManager));
                if (uiPopupManagerInstanceField == null)
                {
                    VRCModLogger.Log("[VRCToolsUpdater] uiPopupManagerInstanceField == null");
                    yield break;
                }
                uiPopupManagerInstance = uiPopupManagerInstanceField.GetValue(null) as VRCUiPopupManager;
            }

            if (uiPopupManagerInstance == null)
            {
                VRCModLogger.Log("[VRCToolsUpdater] uiPopupManagerInstance == null");
                yield break;
            }

            uiPopupManagerInstance.ShowStandardPopup(title, body, leftButton, leftButtonAction, rightButton, rightButtonAction, additionnalSetup);
        }

        private static IEnumerator ShowPopup(string title, string body, Action<VRCUiPopup> additionnalSetup = null)
        {
            VRCModLogger.Log("ShowPopup");
            yield return WaitForUIManager();
            if (uiPopupManagerInstance == null)
            {
                FieldInfo[] nonpublicStaticPopupFields = typeof(VRCUiPopupManager).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
                if (nonpublicStaticPopupFields.Length == 0)
                {
                    VRCModLogger.Log("[VRCToolsUpdater] nonpublicStaticPopupFields.Length == 0");
                    yield break;
                }
                FieldInfo uiPopupManagerInstanceField = nonpublicStaticPopupFields.First(field => field.FieldType == typeof(VRCUiPopupManager));
                if (uiPopupManagerInstanceField == null)
                {
                    VRCModLogger.Log("[VRCToolsUpdater] uiPopupManagerInstanceField == null");
                    yield break;
                }
                uiPopupManagerInstance = uiPopupManagerInstanceField.GetValue(null) as VRCUiPopupManager;
            }

            if (uiPopupManagerInstance == null)
            {
                VRCModLogger.Log("[VRCToolsUpdater] uiPopupManagerInstance == null");
                yield break;
            }

            uiPopupManagerInstance.ShowStandardPopup(title, body, additionnalSetup);
        }

        private static IEnumerator ShowUpdatePopup()
        {
            /*
            uiManagerInstance.HideScreen("SCREEN");
            //uiManagerInstance.ShowScreen("UserInterface/MenuContent/Screens/UpdateRequired");
            //uiManagerInstance.ShowScreen("UserInterface/MenuContent/Screens/Title");
            //uiManagerInstance.ShowScreen("UserInterface/MenuContent/Popups/LoadingPopup");
            //uiManagerInstance.ShowScreen("UserInterface/MenuContent/Backdrop/Backdrop");
            */
            VRCModLogger.Log("[VRCToolsUpdater] Showing update popup");
            yield return ShowPopup("VRCTools Updater", "Updating VRCTools", "Quit", () => Application.Quit(), (popup) => {
                if(popup.popupProgressFillImage != null)
                {
                    popup.popupProgressFillImage.enabled = true;
                    popup.popupProgressFillImage.fillAmount = 0f;
                    downloadProgressFillImage = popup.popupProgressFillImage;
                }
            });
        }

        private static IEnumerator ShowErrorPopup(string error)
        {
            VRCModLogger.Log("[VRCToolsUpdater] Showing error popup");
            yield return ShowPopup("VRCTools Updater", error, "Quit", () => Application.Quit(), (popup) => {
                if (popup.popupProgressFillImage != null)
                {
                    popup.popupProgressFillImage.enabled = true;
                    popup.popupProgressFillImage.fillAmount = 0f;
                    downloadProgressFillImage = popup.popupProgressFillImage;
                }
            });
        }




        public static int getResponseCode(WebClient request)
        {
            int ret = 0;
            if (request.ResponseHeaders == null)
            {
                Debug.LogError("no response headers.");
            }
            else
            {
                if (!request.ResponseHeaders.AllKeys.Contains("STATUS"))
                {
                    Debug.LogError("response headers has no STATUS.");
                }
                else
                {
                    ret = parseResponseCode(request.ResponseHeaders["STATUS"]);
                }
            }

            return ret;
        }

        public static int getResponseCode(WWW request)
        {
            int ret = 0;
            if (request.responseHeaders == null)
            {
                Debug.LogError("no response headers.");
            }
            else
            {
                if (!request.responseHeaders.ContainsKey("STATUS"))
                {
                    Debug.LogError("response headers has no STATUS.");
                }
                else
                {
                    ret = parseResponseCode(request.responseHeaders["STATUS"]);
                }
            }

            return ret;
        }

        public static int parseResponseCode(string statusLine)
        {
            int ret = 0;

            string[] components = statusLine.Split(' ');
            if (components.Length < 3)
            {
                Debug.LogError("invalid response status: " + statusLine);
            }
            else
            {
                if (!int.TryParse(components[1], out ret))
                {
                    Debug.LogError("invalid response code: " + components[1]);
                }
            }

            return ret;
        }
    }
}