using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessCloser
{
    internal class Program
    {
        static List<string> processNames;
        static string settingsFilePath;

        [DllImport("kernel32.dll")]
        static public extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;

        static string mainAppPath = Environment.CurrentDirectory + "\\ProcessCloser.exe";
        static void Main(string[] args)
        {
            ShowWindow(GetConsoleWindow(), SW_SHOW);

            ProcessKiller(Process.GetProcessesByName("ProcessCloser"));

            if(!File.Exists(mainAppPath))
            {
                Console.WriteLine("Файла ProcessCloser.exe не существует в текущей директории!");
                Console.ReadLine();
                return;
            }

            settingsFilePath = Environment.CurrentDirectory + "\\ProcessesList.txt";

            if (!File.Exists(settingsFilePath))
                using (var fs = File.Create(settingsFilePath)) { }
            processNames = File.ReadAllLines(settingsFilePath).ToList();

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

            ShowWindow(GetConsoleWindow(), SW_HIDE);

            WriteListToFile();
            
            Process.Start(mainAppPath);

            SetAutoRun(mainAppPath);
        }
        static void SetAutoRun(string path)
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            rkApp.SetValue("ProcessCloser", $"\"{mainAppPath}\"");

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
            File.WriteAllLines(settingsFilePath, processNames);
        }
        static void ProcessKiller(Process[] processesToKill)
        {
            if (processesToKill != null)
            {
                foreach (var process in processesToKill)
                {
                    process.Kill(true);
                }
            }
        }
    }
}
