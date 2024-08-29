using System;
using System.IO;
using Microsoft.Win32;

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
            
            Console.WriteLine("Loading...");
            var activator = new Activator(executablePath);

            activator.ActivateProducts();
            Console.WriteLine("Activated all packages");

            activator.GenerateLicenseSummary();
            Console.WriteLine("Authorized");

            activator.Dispose(); // release the assembly

            Console.ReadLine();

            AssemblyPatcher.PatchExecutable(installFolder);
            AssemblyPatcher.BypassAuthentication(installFolder);

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