using System;
using System.IO;
using UnityEngine;

namespace VRCModLoader
{
    public class VRCModLogger
    {
        internal static bool consoleEnabled = false;
        private static StreamWriter log;

        internal static string CombinePaths(params string[] paths)
        {
            if (paths == null) throw new ArgumentNullException("paths");
            return paths.Aggregate(Path.Combine);
        }

        internal static void Init()
        {
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            //string logFilePath = CombinePaths(Application.persistentDataPath, "Logs", "VRCModLoader");
            string logFilePath = CombinePaths(Environment.CurrentDirectory, "Logs", "VRCModLoader");
            logFilePath = logFilePath + "/VRCModLoader_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + ".log";
            logFileInfo = new FileInfo(logFilePath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            log = new StreamWriter(fileStream);
            log.AutoFlush = true;
        }

        internal static void Stop()
        {
            log.Close();
        }

        public static void Log(string s)
        {
            if(consoleEnabled) Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] [VRCMod] " + s);
            Debug.Log("[VRCMod] " + s);
            if(log != null) log.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + s);
        }

        public static void Log(string s, params object[] args)
        {
            if (consoleEnabled) Console.WriteLine("[VRCMod] " + s, args);
            Debug.LogFormat("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] [VRCMod] " + string.Format(s, args));
            if (log != null) log.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + string.Format(s, args));
        }

        public static void LogError(string s)
        {
            if (consoleEnabled) Console.WriteLine("[VRCMod] [Error] " + s);
            Debug.Log("[VRCMod] [Error] " + s);
            if (log != null) log.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] [Error] " + s);
        }

        public static void LogError(string s, params object[] args)
        {
            if (consoleEnabled) Console.WriteLine("[VRCMod] [Error] " + s, args);
            Debug.LogFormat("[VRCMod] [Error] " + s, args);
            if (log != null) log.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] [Error] " + string.Format(s, args));
        }
    }
}
