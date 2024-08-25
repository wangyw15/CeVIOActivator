using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace CeVIOActivator
{
    public static class AssemblyPatcher
    {
        // System.Void CeVIO.ToolBarControl.ToolBarControl::.cctor()
        private const string TARGET_PATCH_FILE = "CeVIO.ToolBarControl.dll";
        private const string TARGET_PATCH_CLASS = "CeVIO.ToolBarControl.ToolBarControl";
        private const string TARGET_PATCH_METHOD = ".cctor";

        private const string AUTHORIZER_NAME = "CeVIO.Editor.MissionAssistant.Authorizer";
        private const string PRODUCT_LICENSE_NAME = "CeVIO.Editor.MissionAssistant.ProductLicense";

        private const string BACKUP_POSTFIX = ".bak";

        [Obsolete("Directly patch executable will make it not work. Use PatchFile instead.")]
        public static void PatchExecutable(string cevioExecutablePath)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(cevioExecutablePath));

            var asm = AssemblyDefinition.ReadAssembly(cevioExecutablePath, new ReaderParameters
            {
                AssemblyResolver = resolver
            });
            var type = asm.MainModule.GetType("CeVIO.Editor.MissionAssistant.Authorizer");
            var setter = type.Properties.First(x => x.Name == "HasAuthorized").SetMethod;
            var method = type.Methods.First(x => x.Name == "Authorize");
            var processor = method.Body.GetILProcessor();
            processor.Replace(method.Body.Instructions[2], processor.Create(OpCodes.Nop));
            processor.Replace(method.Body.Instructions[3], processor.Create(OpCodes.Ldc_I4_1));
            processor.Replace(method.Body.Instructions[4], processor.Create(OpCodes.Call, setter));
            asm.Write("CeVIO AI.exe");
        }
        public static bool CheckPatched(string cevioInstallPath)
        {
            var modulePath = Path.Combine(cevioInstallPath, TARGET_PATCH_FILE);
            if (!File.Exists(modulePath))
            {
                throw new FileNotFoundException($"{TARGET_PATCH_FILE} not found");
            }

            var module = ModuleDefinition.ReadModule(modulePath);
            var type = module.GetType(TARGET_PATCH_CLASS);
            var method = type.Methods.First(m => m.Name == TARGET_PATCH_METHOD);

            return method.Body.Instructions.Any(x => x.Operand as string == AUTHORIZER_NAME);
        }

        public static bool PatchFile(string cevioInstallPath, TimeSpan? MaxOfflineDuration = null)
        {
            if (MaxOfflineDuration == null)
            {
                MaxOfflineDuration = TimeSpan.FromDays(365);
            }

            var modulePath = Path.Combine(cevioInstallPath, TARGET_PATCH_FILE);
            if (!File.Exists(modulePath))
            {
                throw new FileNotFoundException($"{TARGET_PATCH_FILE} not found");
            }

            // find method
            var module = ModuleDefinition.ReadModule(modulePath);
            var type = module.GetType(TARGET_PATCH_CLASS);
            var method = type.Methods.First(m => m.Name == TARGET_PATCH_METHOD);

            // detect if patched
            if (method.Body.Instructions.Any(x => x.Operand as string == AUTHORIZER_NAME))
            {
                return false;
            }

            // generate instructions
            var processor = method.Body.GetILProcessor();
            var instructions = new Instruction[]
            {
                // System.Reflection.Assembly.GetEntryAssembly().GetType("CeVIO.Editor.MissionAssistant.Authorizer").GetProperty("HasAuthorized").SetValue(null, true);
                processor.Create(OpCodes.Call, module.ImportReference(typeof(Assembly).GetMethod("GetEntryAssembly"))),
                processor.Create(OpCodes.Ldstr, AUTHORIZER_NAME),
                processor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Assembly).GetMethod("GetType", new Type[] { typeof(string) }))),
                processor.Create(OpCodes.Ldstr, "HasAuthorized"),
                processor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Type).GetMethod("GetProperty", new Type[] { typeof(string) }))),
                processor.Create(OpCodes.Ldnull),
                processor.Create(OpCodes.Ldc_I4_1),
                processor.Create(OpCodes.Box, module.ImportReference(typeof(bool))),
                processor.Create(OpCodes.Callvirt, module.ImportReference(typeof(PropertyInfo).GetMethod("SetValue", new Type[] { typeof(object), typeof(object) }))),

                // Assembly.GetEntryAssembly().GetType("CeVIO.Editor.MissionAssistant.ProductLicense").GetField("OfflineAcceptablePeriod").SetValue(null, TimeSpan.FromDays(ActivateDurationDays));
                // currently it is not usable
                processor.Create(OpCodes.Call, module.ImportReference(typeof(Assembly).GetMethod("GetEntryAssembly"))),
                processor.Create(OpCodes.Ldstr, PRODUCT_LICENSE_NAME),
                processor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Assembly).GetMethod("GetType", new Type[] { typeof(string) }))),
                processor.Create(OpCodes.Ldstr, "OfflineAcceptablePeriod"),
                processor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Type).GetMethod("GetField", new Type[] { typeof(string) }))),
                processor.Create(OpCodes.Ldnull),
                processor.Create(OpCodes.Ldc_R8, MaxOfflineDuration.Value.TotalDays),
                processor.Create(OpCodes.Call, module.ImportReference(typeof(TimeSpan).GetMethod("FromDays", new Type[] { typeof(double) }))),
                processor.Create(OpCodes.Box, module.ImportReference(typeof(TimeSpan))),
                processor.Create(OpCodes.Callvirt, module.ImportReference(typeof(PropertyInfo).GetMethod("SetValue", new Type[] { typeof(object), typeof(object) }))),
                
                // typeof(DateTime).GetField("MaxValue").SetValue(null, DateTime.Now);
                processor.Create(OpCodes.Ldtoken, module.ImportReference(typeof(DateTime))),
                processor.Create(OpCodes.Call, module.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle"))),
                processor.Create(OpCodes.Ldstr, "MaxValue"),
                processor.Create(OpCodes.Call, module.ImportReference(typeof(Type).GetMethod("GetField", new Type[] { typeof(string) }))),
                processor.Create(OpCodes.Ldnull),
                processor.Create(OpCodes.Call, module.ImportReference(typeof(DateTime).GetMethod("get_Now"))),
                processor.Create(OpCodes.Box, module.ImportReference(typeof(DateTime))),
                processor.Create(OpCodes.Callvirt, module.ImportReference(typeof(FieldInfo).GetMethod("SetValue", new Type[] { typeof(object), typeof(object) }))),
            };

            // patch
            for (var i = instructions.Length - 1; i >= 0; i--)
            {
                processor.InsertBefore(method.Body.Instructions[0], instructions[i]);
            }

            // remove BeforeFieldInit flag
            type.Attributes &= ~TypeAttributes.BeforeFieldInit;
            
            // write
            module.Write(TARGET_PATCH_FILE);

            return true;
        }

        public static void DeleteNgen(string cevioInstallPath)
        {
            foreach (var i in Directory.GetFiles(cevioInstallPath))
            {
                if (!Regex.IsMatch(Path.GetFileName(i), @"cevio.*\.(?:exe|dll)", RegexOptions.IgnoreCase))
                {
                    continue;
                }
                var process = new Process();
                process.StartInfo.FileName = "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\ngen.exe";
                process.StartInfo.Arguments = $"uninstall \"{i}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
                Console.WriteLine("ngen uninstalled " + i);
            }
        }

        public static void ReplaceFile(string cevioInstallPath)
        {
            var sourcePath = Path.GetFullPath(TARGET_PATCH_FILE);
            var targetPath = Path.Combine(cevioInstallPath, TARGET_PATCH_FILE);
            // backup unmodified file
            if (!File.Exists(targetPath + BACKUP_POSTFIX))
            {
                File.Copy(targetPath, targetPath + BACKUP_POSTFIX, true);
            }
            // replace
            File.Copy(sourcePath, targetPath, true);
            // delete source
            File.Delete(sourcePath);

            // old method by cmd
            //var process = new Process();
            //process.StartInfo.FileName = "cmd.exe";
            //process.StartInfo.Arguments = $"/c \"timeout 1 /nobreak & copy /y \"{targetPath}\" \"{targetPath}.bak\" & copy /y \"{sourcePath}\" \"{targetPath}\" & del \"{sourcePath}\" & echo Completed & pause\"";
            //process.StartInfo.UseShellExecute = false;
            //process.Start();
        }
    }
}
