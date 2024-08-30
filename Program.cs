using System;
using System.IO;
using Microsoft.Win32;
using CeVIOActivator.Libs;

using Activator = CeVIOActivator.Libs.Activator;

namespace CeVIOActivator
{
    class Program
    {
        static void Main(string[] args)
        {
            var installFolder = GetCeVIOInstallFolder();

            var executablePath = GetCeVIOExecutable();
            if (string.IsNullOrEmpty(executablePath))
            {
                Console.Write("CeVIO AI.exe not found, please specify the file: ");
                executablePath = (Console.ReadLine() ?? "").Replace("\"", "");
            }

            Console.WriteLine("Start patching...");

            Console.WriteLine("Writing registry...");
            var assembly = new CeVIOAssemblyManager(executablePath);
            var activator = assembly.CreateInstance<Activator>();

            // create instances
            activator.Initialize(assembly);

            activator.ActivateProducts();
            Console.WriteLine("Activated all packages");

            activator.GenerateLicenseSummary();
            Console.WriteLine("Authorized");

            assembly.Dispose(); // release the assembly

            AssemblyPatcher.BypassAuthentication(installFolder);
            AssemblyPatcher.PatchExecutable(installFolder);

            Console.WriteLine("Deleting Ngen");
            AssemblyPatcher.DeleteNgen(installFolder);

            Console.WriteLine("Activate complete");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static string GetCeVIOInstallFolder()
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

        private static string GetCeVIOExecutable()
        {
            var folder = GetCeVIOInstallFolder();

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