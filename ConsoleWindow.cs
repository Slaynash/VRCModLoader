using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

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

        private static void ShowConsole()
        {
            SetForegroundWindow(GetConsoleWindow());
        }

        internal static void CreateConsole()
        {
            AllocConsole();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            Console.Clear();
            Console.Title = "VRLoader by Harekuin";
            ShowConsole();
        }
    }
}