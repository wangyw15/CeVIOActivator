using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CeVIOActivator.Core.Patches;

namespace CeVIOActivator.Core
{
    public static class AssemblyPatcher
    {
        private const string BACKUP_POSTFIX = ".bak";

        public static List<Type> GetPatches()
        {
            var asm = Assembly.GetExecutingAssembly();
            return asm.GetTypes().Where(
                t => t.IsClass && t.GetInterfaces().Any(i => i.Name == nameof(ICeVIOPatch))
                ).ToList();
        }

        private static void MakeBackup(string filename)
        {
            var sourcePath = Path.Combine(filename);
            var backupPath = Path.Combine(filename + BACKUP_POSTFIX);

            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"{filename} Not found");
                return;
            }

            if (File.Exists(backupPath))
            {
                Console.WriteLine("Skip backup: " + backupPath + " already exists");
                return;
            }

            Console.WriteLine("Backup: " + filename);
            File.Copy(sourcePath, backupPath);
        }

        public static void Patch(string cevioInstallPath, CeVIOVersion version, bool dryrun = false)
        {
            var allPatches = from p in GetPatches() select (ICeVIOPatch)Activator.CreateInstance(p);
            var patches = from p in allPatches where (p.TargetVersion & version) == version select p;
            var targetFiles = (from p in patches select p.TargetAssembly).Distinct();

            foreach (var filename in targetFiles)
            {
                var modulePath = Path.Combine(cevioInstallPath, filename);
                var module = ModuleDefMD.Load(File.ReadAllBytes(modulePath));
                var currentFileTouched = false;

                // find corresponding patches
                var currentPatches = patches.Where(p => p.TargetAssembly == filename);
                foreach (var patch in currentPatches)
                {
                    if (patch is ICeVIOMethodPatch methodPatch)
                    {
                        var method = module
                            .Types.Single(m => m.FullName == methodPatch.TargetType)
                            .FindMethod(methodPatch.TargetMethod);
                        if (methodPatch.AlreadyPatched(method))
                        {
                            Console.WriteLine("Skip patch: " + patch.GetType().Name);
                        }
                        else
                        {
                            Console.WriteLine("Applying patch: " + patch.GetType().Name);
                            methodPatch.Patch(method);
                            currentFileTouched |= true;
                            method.Body.OptimizeMacros();
                            method.Body.OptimizeBranches();
                        }
                    }
                    else if (patch is ICeVIOPropertyPatch propertyPatch)
                    {
                        var property = module
                            .Types.Single(m => m.FullName == propertyPatch.TargetType)
                            .FindProperty(propertyPatch.TargetProperty);
                        if (propertyPatch.AlreadyPatched(property))
                        {
                            Console.WriteLine("Skip patch: " + patch.GetType().Name);
                        }
                        else
                        {
                            Console.WriteLine("Applying patch: " + patch.GetType().Name);
                            propertyPatch.Patch(property);
                            currentFileTouched |= true;
                        }
                    }
                }

                if (dryrun || !currentFileTouched)
                {
                    continue;
                }

                MakeBackup(modulePath);
                module.Save(modulePath);
                DeleteNgen(modulePath);
            }
        }

        public static void DeleteNgen(string filename)
        {
            Console.WriteLine("Uninstall ngen: " + filename);

            var process = new Process();
            process.StartInfo.FileName = "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\ngen.exe";
            process.StartInfo.Arguments = $"uninstall \"{filename}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
        }
    }
}
