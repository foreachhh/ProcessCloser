using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessCloser
{
    internal class Program
    {
        static List<string> processNames;
        static string baseDir = AppContext.BaseDirectory;
        static string processesListPath = Path.Combine(baseDir, "ProcessesList.txt");
        static int checkInterval = 500;

        [DllImport("kernel32.dll")]
        static public extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;
        static void Main(string[] args)
        {
            Process current = Process.GetCurrentProcess();
            Process[] oldProcesses = Process.GetProcessesByName(current.ProcessName)
                .Where(p => p.Id != current.Id).ToArray();                                  // Здесь типо отбираются все процессы с таким же именем, кроме себя самого.

            if (oldProcesses.Length > 0)
                ProcessKiller(oldProcesses);

            if (!File.Exists(processesListPath))
                using (var fs = File.Create(processesListPath)) { }

            if (args.Length == 0 || (args.Length > 0 && args[0] != "-service"))
                Launcher();

                ShowWindow(GetConsoleWindow(), SW_HIDE);

            SetAutoRun(baseDir + "ProcessCloser.exe");

            ProcessChecker();
        }
        static void Launcher()
        {
            processNames = File.ReadAllLines(processesListPath).ToList();

            ShowWindow(GetConsoleWindow(), SW_SHOW);

            while (true)
            {
                ListToConsole();

                Console.WriteLine("\nВведите соответствующую цифры для изменения списка. Для продолжения нажмите Enter.\n" +
                    "0 - Удалить из списка\n" +
                    "1 - Добавить в список\n");
                string input = Console.ReadLine();

                if (input.ToLower() == "0")
                    DeleteFromList();
                else if (input.ToLower() == "1")
                    AddToList();
                else if (string.IsNullOrWhiteSpace(input))
                    break;
            }
            WriteListToFile();
        }
        static void SetAutoRun(string path)
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            rkApp.SetValue("ProcessCloser", $"\"{path}\" -service");

            rkApp.Close();
        }
        static void ListToConsole()
        {
            Console.Clear();
            Console.WriteLine("Список всех текущих процессов для закрытия:");
            for (int i = 0; i < processNames.Count; i++)
            {
                Console.Write($"{i}. {processNames[i]}\n");
            }
        }
        static void AddToList()
        {
            while (true)
            {
                ListToConsole();
                Console.WriteLine("\nПоследовательно вводите названия процессов. Для продолжения нажмите Enter.");
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    processNames.Add(input);
                }
                else
                {
                    break;
                }
            }
        }
        static void DeleteFromList()
        {
            while (true)
            {
                ListToConsole();
                Console.WriteLine("\nВведите номер элемента который хотите удалить. Для продолжения нажмите Enter.");
                string input = Console.ReadLine();
                int index;
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (Int32.TryParse(input, out index) & index < processNames.Count)
                    {
                        processNames.RemoveAt(index);

                    }
                    else
                    {
                        Console.WriteLine("Некорректный ввод. Нажмите любую клавишу и попробуйте ещё раз.");
                        Console.Read();
                    }
                }
                else
                {
                    break;
                }
            }
        }
        static void WriteListToFile()
        {
            File.WriteAllLines(processesListPath, processNames);
        }
        public static void ProcessChecker()
        {
            processNames = File.ReadAllLines(processesListPath).ToList();
            if (processNames.Count == 0)
                return;

            Process[] processesToKill;
            while (true)
            {
                Thread.Sleep(checkInterval);
                for (int i = 0; i < processNames.Count; i++)
                    if ((processesToKill = Process.GetProcessesByName(processNames[i])) != null)
                        ProcessKiller(processesToKill);
            }
        }
        static void ProcessKiller(Process[] processesToKill)
        {
            if (processesToKill != null)
            {
                foreach (var process in processesToKill)
                {
                    process.Kill(true);
                    process.WaitForExit();
                }
            }
        }
    }
}
