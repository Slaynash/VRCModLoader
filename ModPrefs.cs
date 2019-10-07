using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VRCModLoader
{
    public class Pref
    {
        public string section { get; set; } = "";
        public string name { get; set; } = "";
        public object value { get; set; } = null;
    }
    /// <summary>
    /// Allows to get and set preferences for your mod. 
    /// </summary>
    [Obsolete("Please use VRCTools.ModPrefs instead")]
    public static class ModPrefs
    {
        /*private static IniFile _instance;
        private static IniFile Instance
        {
            get
            {
                if (_instance == null)
                {
                    string userDataDir = Path.Combine(Environment.CurrentDirectory, "UserData");
                    if (!Directory.Exists(userDataDir)) Directory.CreateDirectory(userDataDir);

                    _instance = new IniFile(Path.Combine(userDataDir, "modprefs.ini"));
                }
                return _instance;
            }
        }*/
        private static List<Pref> _prefList;
        private static List<Pref> prefList
        {
            get
            {
                if (_prefList == null && UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsPlayer)
                {
                    string userDataDir = Path.Combine(Environment.CurrentDirectory, "UserData");
                    if (!Directory.Exists(userDataDir)) Directory.CreateDirectory(userDataDir);
                    if (!File.Exists(Path.Combine(userDataDir, "modPrefs.json")))
                    {
                        _prefList = new List<Pref>();
                        File.WriteAllText(Path.Combine(userDataDir, "modPrefs.json"), JsonConvert.SerializeObject(_prefList, Formatting.Indented));
                    }
                    var input = File.ReadAllText(Path.Combine(userDataDir, "modPrefs.json"));
                    _prefList = JsonConvert.DeserializeObject<List<Pref>>(input);
                }
                if (_prefList == null && UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android)
                {
                    string userDataDir = "/sdcard/VRCTools/UserData";
                    if (!Directory.Exists(userDataDir)) Directory.CreateDirectory(userDataDir);
                    if (!File.Exists(Path.Combine(userDataDir, "modPrefs.json")))
                    {
                        _prefList = new List<Pref>();
                        File.WriteAllText(Path.Combine(userDataDir, "modPrefs.json"), JsonConvert.SerializeObject(_prefList, Formatting.Indented));
                    }
                    var input = File.ReadAllText(Path.Combine(userDataDir, "modPrefs.json"));
                    _prefList = JsonConvert.DeserializeObject<List<Pref>>(input);
                }
                return _prefList;
            }
        }

        private static void WriteJson()
        {
            string configPath = "";
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsPlayer)
            {
               configPath = Path.Combine(Environment.CurrentDirectory, "UserData");
                if (!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);
            }
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android)
            {
                configPath = "/sdcard/VRCTools/UserData";
                if (!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);
            }
            if (File.Exists(Path.Combine(configPath, "modPrefs.json.tmp")))
                File.Delete(Path.Combine(configPath, "modPrefs.json.tmp"));
            File.WriteAllText(Path.Combine(configPath, "modPrefs.json.tmp"), JsonConvert.SerializeObject(prefList, Formatting.Indented));
            if (File.Exists(Path.Combine(configPath, "modPrefs.json")))
                File.Delete(Path.Combine(configPath, "modPrefs.json"));
            File.Move(Path.Combine(configPath, "modPrefs.json.tmp"), Path.Combine(configPath, "modPrefs.json"));
        }
        /// <summary>
        /// Gets a string from the ini.
        /// </summary>
        /// <param name="section">Section of the key.</param>
        /// <param name="name">Name of the key.</param>
        /// <param name="defaultValue">Value that should be used when no value is found.</param>
        /// <param name="autoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        public static string GetString(string section, string name, string defaultValue = "", bool autoSave = false)
        {
            //string value = Instance.IniReadValue(section, name);
            string value = (string)prefList.Find(i => i.section == section && i.name == name).value;
            if (value != null && value != "")
                return value;
            else if (autoSave)
                SetString(section, name, defaultValue);

            return defaultValue;
        }

        /// <summary>
        /// Gets an int from the ini.
        /// </summary>
        /// <param name="section">Section of the key.</param>
        /// <param name="name">Name of the key.</param>
        /// <param name="defaultValue">Value that should be used when no value is found.</param>
        /// <param name="autoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        public static int GetInt(string section, string name, int defaultValue = 0, bool autoSave = false)
        {
            int value;
            if (int.TryParse((string)prefList.Find(i => i.section == section && i.name == name).value, out value))
                return value;
            else if (autoSave)
                SetInt(section, name, defaultValue);
                
            return defaultValue;
        }


        /// <summary>
        /// Gets a float from the ini.
        /// </summary>
        /// <param name="section">Section of the key.</param>
        /// <param name="name">Name of the key.</param>
        /// <param name="defaultValue">Value that should be used when no value is found.</param>
        /// <param name="autoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        public static float GetFloat(string section, string name, float defaultValue = 0f, bool autoSave = false)
        {
            float value;
            if (float.TryParse((string)prefList.Find(i => i.section == section && i.name == name).value, out value))
                return value;
            else if (autoSave)
                SetFloat(section, name, defaultValue);

            return defaultValue;
        }

        /// <summary>
        /// Gets a bool from the ini.
        /// </summary>
        /// <param name="section">Section of the key.</param>
        /// <param name="name">Name of the key.</param>
        /// <param name="defaultValue">Value that should be used when no value is found.</param>
        /// <param name="autoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        public static bool GetBool(string section, string name, bool defaultValue = false, bool autoSave = false)
        {
            /*string sVal = GetString(section, name, null);
            if (sVal == "1" || sVal == "0")
            {
                return sVal == "1";*/
            bool value = (bool)prefList.Find(i => i.section == section && i.name == name).value;
            if (value != null)
            {
                return value;
            }
            else if (autoSave)
            {
                SetBool(section, name, defaultValue);
            }

            return defaultValue;
        }


        /// <summary>
        /// Checks whether or not a key exists in the ini.
        /// </summary>
        /// <param name="section">Section of the key.</param>
        /// <param name="name">Name of the key.</param>
        /// <returns></returns>
        public static bool HasKey(string section, string name)
        {
            if (prefList.Find(i => i.section == section && i.name == name) != null)
                return true;
            else
                return false;
            //return Instance.IniReadValue(section, name) != null;
        }

        /// <summary>
        /// Sets a float in the ini.
        /// </summary>
        /// <param name="section">Section of the key.</param>
        /// <param name="name">Name of the key.</param>
        /// <param name="value">Value that should be written.</param>
        public static void SetFloat(string section, string name, float value)
        {
            if (prefList.Find(i => i.section == section && i.name == name) != null)
                prefList.Find(i => i.section == section && i.name == name).value = value.ToString();
            else
                prefList.Add(new Pref(){ section = section, name = name, value = value });
            WriteJson();
        }

        /// <summary>
        /// Sets an int in the ini.
        /// </summary>
        /// <param name="section">Section of the key.</param>
        /// <param name="name">Name of the key.</param>
        /// <param name="value">Value that should be written.</param>
        public static void SetInt(string section, string name, int value)
        {
            if (prefList.Find(i => i.section == section && i.name == name) != null)
                prefList.Find(i => i.section == section && i.name == name).value = value.ToString();
            else
                prefList.Add(new Pref() { section = section, name = name, value = value });
            WriteJson();
        }

        /// <summary>
        /// Sets a string in the ini.
        /// </summary>
        /// <param name="section">Section of the key.</param>
        /// <param name="name">Name of the key.</param>
        /// <param name="value">Value that should be written.</param>
        public static void SetString(string section, string name, string value)
        {
            if (prefList.Find(i => i.section == section && i.name == name) != null)
                prefList.Find(i => i.section == section && i.name == name).value = value;
            else
                prefList.Add(new Pref() { section = section, name = name, value = value });
            WriteJson();
        }

        /// <summary>
        /// Sets a bool in the ini.
        /// </summary>
        /// <param name="section">Section of the key.</param>
        /// <param name="name">Name of the key.</param>
        /// <param name="value">Value that should be written.</param>
        public static void SetBool(string section, string name, bool value)
        {
            if (prefList.Find(i => i.section == section && i.name == name) != null)
                prefList.Find(i => i.section == section && i.name == name).value = value;
            else
                prefList.Add(new Pref() { section = section, name = name, value = value });
            WriteJson();
        }
    }
}
