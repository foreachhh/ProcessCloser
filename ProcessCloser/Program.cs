using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessCloser
{
    internal class Program
    {
        static string[] processesList;
        static string settingsFileName = "\\ProcessesList.txt";

        static int checkInterval = 500;

        [DllImport("kernel32.dll")]
        static public extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main(string[] args)
        {
            ShowWindow(GetConsoleWindow(), 0); // Вызов метода который влияет на видимость консоли. Второй параметр(0) - означает что консоль должна быть скрыта.

            processesList = File.ReadAllLines(Environment.CurrentDirectory + settingsFileName);

            ProcessChecker();
        }
        public static void ProcessChecker()
        {
            if (processesList.Length == 0)
                return;

            Process[] processesToKill;
            while (true)
            {
                Thread.Sleep(checkInterval);
                for (int i = 0; i < processesList.Length; i++)
                    if ((processesToKill = Process.GetProcessesByName(processesList[i])) != null)
                        ProcessKiller(processesToKill);
            }
        }
        static void ProcessKiller(Process[] processesToKill)
        {
            foreach (var process in processesToKill)
            {
                process.Kill(true);
            }
        }
    }
}
