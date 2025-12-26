using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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

            // start program
            var process = new Process();
            process.StartInfo.FileName = executable;
            process.Start();

            var libPath = "CeVIOActivator.Patcher.dll";
            var libFullPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            libFullPath = Path.Combine(libFullPath, libPath);

            // inject and patch
            var injector = new Injector(process.Handle);
            // inject
            Console.WriteLine("Injecting...");

            var hModule = injector.Inject(libFullPath);
            if (hModule == IntPtr.Zero)
            {
                Console.WriteLine("Inject failed");
                Console.ReadKey();
                return;
            }
            Console.WriteLine($"Injected, module address: {hModule}");

            // patch
            Console.WriteLine("Patching...");
            var code = injector.Call(hModule, "Patch");
            if (code != 771)
            {
                Console.WriteLine("Patch failed");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Patch complete");
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
