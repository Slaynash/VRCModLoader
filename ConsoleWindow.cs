using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Windows
{

    class GuiConsole 
    {
        [DllImport("kernel32.dll")]
        private static extern int AllocConsole();

        [DllImport("user32.dll")]
        [return: MarshalAs(2)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        public static extern bool SetWindowText(IntPtr hwnd, String lpString);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string className, string windowName);

        public static string Title = $"{Application.productName} v{Application.version} on {Enum.GetName(typeof(RuntimePlatform), Application.platform).Replace("Player", "")} [VRCModloader]";

        private static void ShowConsole()
        {
            SetForegroundWindow(GetConsoleWindow());
        }

        internal static void CreateConsole()
        {
            AllocConsole();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            // Console.Clear(); Maybe something else wrote to Console before.
            Console.Title = Title;
            ShowConsole();
        }
    }
}
