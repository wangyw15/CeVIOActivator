using CeVIOActivator.Core;
using System;

namespace CeVIOActivator.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Author: wangyw15");
            Console.WriteLine("GitHub: https://github.com/wangyw15/CeVIOActivator");
            Console.WriteLine("Only for study purpose");
            Console.WriteLine();

            var installFolder = CeVIOHelper.GetCeVIOAIInstallFolder();

            var executablePath = CeVIOHelper.GetCeVIOAIExecutable();
            if (string.IsNullOrEmpty(executablePath))
            {
                Console.Write("CeVIO AI.exe not found, please specify the file: ");
                executablePath = (Console.ReadLine() ?? "").Replace("\"", "");
            }

            Console.WriteLine("Start patching...");
            
            AssemblyPatcher.Patch(installFolder, false);

            Console.WriteLine("Patch complete");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}