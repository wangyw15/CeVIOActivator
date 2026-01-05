using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace CeVIOActivator.Loader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // print author
            Console.WriteLine("Author: wangyw15");
            Console.WriteLine("GitHub: https://github.com/wangyw15/CeVIOActivator");
            Console.WriteLine("Only for study purpose");
            Console.WriteLine();

            // determine version
            var aiExecutable = CeVIOHelper.GetCeVIOAIExecutable(CeVIOVersion.AI);
            var csExecutable = CeVIOHelper.GetCeVIOAIExecutable(CeVIOVersion.CS);
            var executable = "";

            if (aiExecutable == null && csExecutable == null)
            {
                Console.WriteLine("Both CeVIO AI and CeVIO CS not found");
                return;
            }
            else if (aiExecutable != null && csExecutable == null)
            {
                executable = aiExecutable;
                Console.WriteLine("Start CeVIO AI");
            }
            else if (aiExecutable == null && csExecutable != null)
            {
                executable = csExecutable;
                Console.WriteLine("Start CeVIO CS");
            }
            else
            {
                Console.WriteLine("1. CeVIO AI");
                Console.WriteLine("2. CeVIO CS");
                Console.WriteLine("0. Exit");
                while (true)
                {
                    Console.Write("Select version to start (default 1): ");
                    var choice = Console.ReadLine();
                    if (choice.Trim() == "0")
                    {
                        return;
                    }
                    else if (choice.Trim() == "1" || choice.Trim() == "")
                    {
                        executable = aiExecutable;
                        Console.WriteLine("Start CeVIO AI");
                        break;
                    }
                    else if (choice.Trim() == "2")
                    {
                        executable = csExecutable;
                        Console.WriteLine("Start CeVIO CS");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice!");
                    }
                }
            }

            // replace origin exe
            if (Assembly.GetExecutingAssembly().Location == executable)
            {
                Console.WriteLine("Working in CeVIO executable replacement mode");
                executable = Path.Combine(Path.GetDirectoryName(executable), Path.GetFileNameWithoutExtension(executable) + ".orig.exe");
            }

            // passthrough arguments
            var arguments = "";
            foreach (var arg in args)
            {
                arguments += $"{arg} ";
            }
            arguments = arguments.Trim();
            arguments = string.Join(" ", args);

            Console.WriteLine("Launching: " + executable);
            Console.WriteLine("With arguments: " + arguments);

            // start program
            var process = new Process();
            process.StartInfo.FileName = executable;
            process.StartInfo.Arguments = arguments;
            process.Start();

            Patch(process);

            Console.WriteLine("This window will close after 3 seconds...");
            Thread.Sleep(3 * 1000);
        }

        public static void Patch(Process process)
        {
            // check inject dll
            var libName = "CeVIOActivator.Patcher.dll";
            var libPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), libName);
            if (!File.Exists(libPath))
            {
                Console.WriteLine($"Unable to find {libName}");
                return;
            }

            // inject and patch
            using (var reloaded = new Reloaded.Injector.Injector(process))
            {
                var moduleAddr = reloaded.Inject(libPath);
                if (moduleAddr <= 0)
                {
                    Console.WriteLine("Inject failed");
                    return;
                }
                Console.WriteLine("Injected");

                var code = reloaded.CallFunction<int>(libPath, "Patch");
                if (code != 771)
                {
                    Console.WriteLine("Patch failed");
                    return;
                }
                Console.WriteLine("Patched");
            }
        }
    }

    public enum CeVIOVersion
    {
        AI,
        CS,
    }

    public class CeVIOHelper
    {
        /// <summary>
        /// Get the installation folder of CeVIO
        /// </summary>
        /// <returns>The installation folder of CeVIO</returns>
        public static string GetCeVIOAIInstallFolder(CeVIOVersion version)
        {
            if (version == CeVIOVersion.AI)
            {
                using (var reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\CeVIO_NV\\Subject\\Editor\\x64"))
                {
                    if (reg == null)
                    {
                        return null;
                    }
                    return reg.GetValue("InstallFolder") as string;
                }
            }
            else if (version == CeVIOVersion.CS)
            {
                using (var reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\CeVIO\\Subject\\Editor\\x64"))
                {
                    if (reg == null)
                    {
                        return null;
                    }
                    return reg.GetValue("InstallFolder") as string;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the executable path of CeVIO
        /// </summary>
        /// <returns>The executable path of CeVIO</returns>
        public static string GetCeVIOAIExecutable(CeVIOVersion version)
        {
            var folder = GetCeVIOAIInstallFolder(version);

            if (string.IsNullOrEmpty(folder))
            {
                return null;
            }

            foreach (var file in Directory.GetFiles(folder))
            {
                if (Path.GetExtension(file).ToLower() == ".exe" && Path.GetFileName(file).StartsWith("CeVIO"))
                {
                    return file;
                }
            }

            return null;
        }
    }
}
