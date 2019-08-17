using System;
using System.IO;
using UnityEngine;

namespace VRCModLoader
{
    public class VRCModLogger
    {
        internal static bool consoleEnabled = false;
        private static StreamWriter log;

        internal static void Init()
        {
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            string logFilePath = Path.Combine(Environment.CurrentDirectory, "Logs");
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
                fileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
            log = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
            log.AutoFlush = true;
        }

        internal static void Stop()
        {
            log.Close();
        }

        internal static string GetTimestamp() { return DateTime.Now.ToString("HH:mm:ss.fff"); }

        public static void Log(string s)
        {
            var timestamp = GetTimestamp();
            if(consoleEnabled) Console.WriteLine("[" + timestamp + "] [VRCMod] " + s);
            if(log != null) log.WriteLine("[" + timestamp + "] " + s);
        }

        public static void Log(string s, params object[] args)
        {
            var timestamp = GetTimestamp();
            var formatted = string.Format(s, args);
            if (consoleEnabled) Console.WriteLine("[" + timestamp + "] [VRCMod] " + s, args);
            if (log != null) log.WriteLine("[" + timestamp + "] " + formatted);
        }

        public static void LogError(string s)
        {
            var timestamp = GetTimestamp();
            if (consoleEnabled) Console.WriteLine("[" + timestamp + "] [VRCMod] [Error] " + s);
            if (log != null) log.WriteLine("[" + timestamp + "] [Error] " + s);
        }

        public static void LogError(string s, params object[] args)
        {
            var timestamp = GetTimestamp();
            var formatted = string.Format(s, args);
            if (consoleEnabled) Console.WriteLine("[" + timestamp + "] [VRCMod] [Error] " + formatted);
            if (log != null) log.WriteLine("[" + timestamp + "] [Error] " + formatted);
        }
    }
}
