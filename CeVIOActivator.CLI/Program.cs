using System;

using CeVIOActivator.Core;

namespace CeVIOActivator.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            // print author
            Console.WriteLine("Author: wangyw15");
            Console.WriteLine("GitHub: https://github.com/wangyw15/CeVIOActivator");
            Console.WriteLine("Only for study purpose");
            Console.WriteLine();

            // select version
            var version = CeVIOVersion.AI;

            Console.WriteLine("1. CeVIO AI");
            Console.WriteLine("2. CeVIO CS7");
            Console.WriteLine("0. Exit");
            while (true)
            {
                Console.Write("Select version to patch (default 1): ");
                var choice = Console.ReadLine();
                if (choice.Trim() == "0")
                {
                    return;
                }
                else if (choice.Trim() == "1")
                {
                    version = CeVIOVersion.AI;
                    break;
                }
                else if (choice.Trim() == "2")
                {
                    version = CeVIOVersion.CS7;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid choice!");
                }
            }

            Console.WriteLine();

            var installFolder = CeVIOHelper.GetCeVIOAIInstallFolder(version);

            var executablePath = CeVIOHelper.GetCeVIOAIExecutable(version);
            if (string.IsNullOrEmpty(executablePath))
            {
                Console.Write("CeVIO executable not found, program exit...");
            }

            Console.WriteLine("Start patching...");
            
            AssemblyPatcher.Patch(installFolder, version, false);

            Console.WriteLine("Patch complete");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}