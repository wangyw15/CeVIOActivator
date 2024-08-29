using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CeVIOActivator
{
    public static class AssemblyPatcher
    {
        private const string BACKUP_POSTFIX = ".bak";

        private static void ReplaceToReturnTrue(MethodDefinition method)
        {
            var processor = method.Body.GetILProcessor();
            processor.Clear();
            processor.Emit(OpCodes.Ldc_I4_1);
            processor.Emit(OpCodes.Ret);
        }

        private static void ClearMethodBody(MethodDefinition method)
        {
            var processor = method.Body.GetILProcessor();
            processor.Clear();
            processor.Emit(OpCodes.Ret);
        }

        private static void MakeBackup(string cevioInstallPath, string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine($"[Backup] {filename} Not found");
                return;
            }

            var backupPath = Path.Combine(cevioInstallPath, filename + BACKUP_POSTFIX);
            if (File.Exists(backupPath))
            {
                Console.WriteLine("[Backup] " + backupPath + " already exists, skip backup");
                return;
            }

            Console.WriteLine("[Backup] " + filename);
            File.Copy(filename, backupPath);
        }

        public static void PatchExecutable(string cevioInstallPath, bool dryrun = false)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(cevioInstallPath);

            // read assembly
            var cevioExecutablePath = Path.Combine(cevioInstallPath, "CeVIO AI.exe");
            var asm = AssemblyDefinition.ReadAssembly(cevioExecutablePath, new ReaderParameters
            {
                AssemblyResolver = resolver
            });

            // CeVIO.Editor.MissionAssistant.Authorizer
            var authorizerType = asm.MainModule.GetType("CeVIO.Editor.MissionAssistant.Authorizer");

            // CeVIO.Editor.MissionAssistant.Authorizer.ForciblyAuthorize
            var forciblyAuthorizeMethod = authorizerType.Methods.Single(m => m.Name == "ForciblyAuthorize");
            {
                // find instruction
                Instruction targetInstruction = null;
                foreach (var instruction in forciblyAuthorizeMethod.Body.Instructions)
                {
                    if (instruction.OpCode == OpCodes.Ldsfld)
                    {
                        var operand = (FieldReference)instruction.Operand;
                        if (operand.FieldType.FullName == typeof(DateTime).FullName &&
                            operand.Name == nameof(DateTime.MaxValue))
                        {
                            targetInstruction = instruction;
                            break;
                        }
                    }
                }

                // replace instruction
                if (targetInstruction != null)
                {
                    Console.WriteLine("[Patch][CeVIO AI.exe] CeVIO.Editor.MissionAssistant.Authorizer.ForciblyAuthorize");

                    var processor = forciblyAuthorizeMethod.Body.GetILProcessor();
                    var replacedInstruction = processor.Create(
                        OpCodes.Call,
                        asm.MainModule.ImportReference(
                            typeof(DateTime).GetProperty(nameof(DateTime.Now)).GetMethod
                            )
                        );
                    processor.Replace(
                        targetInstruction,
                        processor.Create(
                            OpCodes.Call,
                            asm.MainModule.ImportReference(
                                typeof(DateTime).GetProperty(nameof(DateTime.Now)).GetMethod
                                )
                            )
                        );
                }
                else
                {
                    Console.WriteLine("[Skip][CeVIO AI.exe] CeVIO.Editor.MissionAssistant.Authorizer.ForciblyAuthorize");
                }
            }

            // CeVIO.Editor.MissionAssistant.Authorizer.Authorize
            var authorizeMethod = authorizerType.Methods.Single(m => m.Name == "Authorize");
            {
                Instruction targetInstruction = null;
                for (var i = 0; i < authorizerType.Methods.Count; i++)
                {
                    var instruction = authorizeMethod.Body.Instructions[i];

                    if (instruction.OpCode == OpCodes.Throw)
                    {
                        if (authorizeMethod.Body.Instructions[i - 2].OpCode == OpCodes.Call &&
                            authorizeMethod.Body.Instructions[i - 1].OpCode == OpCodes.Newobj)
                        {
                            var operand = (MethodReference)authorizeMethod.Body.Instructions[i - 2].Operand;
                            if (operand.Name == "get_Message_License_Error_Authorization")
                            {
                                targetInstruction = instruction;
                                break;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                // replace instruction
                if (targetInstruction != null)
                {
                    Console.WriteLine("[Patch][CeVIO AI.exe] CeVIO.Editor.MissionAssistant.Authorizer.Authorize");

                    var processor = authorizeMethod.Body.GetILProcessor();
                    var reserveCount = authorizeMethod.Body.Instructions.IndexOf(targetInstruction) + 1;
                    while (authorizeMethod.Body.Instructions.Count > reserveCount)
                    {
                        processor.Remove(authorizeMethod.Body.Instructions[reserveCount]);
                    }
                    processor.Emit(OpCodes.Ldloc_0);
                    processor.Emit(OpCodes.Call, forciblyAuthorizeMethod);
                    processor.Emit(OpCodes.Ret);

                    // add custom code
                    // nop
                    foreach (var i in new int[] { 5, 1, 0 })
                    {
                        processor.RemoveAt(i);
                    }

                    // replace
                    processor.Replace(14, processor.Create(OpCodes.Stloc_0));
                    processor.Replace(16, processor.Create(OpCodes.Ldnull));
                    processor.InsertAfter(16, processor.Create(OpCodes.Ceq));
                    processor.Replace(18, processor.Create(OpCodes.Brfalse_S, authorizeMethod.Body.Instructions[22]));
                    authorizeMethod.Body.ExceptionHandlers.Clear();
                }
                else
                {
                    Console.WriteLine("[Skip][CeVIO AI.exe] CeVIO.Editor.MissionAssistant.Authorizer.Authorize");
                }
            }

            // CeVIO.Editor.MissionAssistant.Authorizer.HasAuthorized.get
            var hasAuthorizedGet = authorizerType.Properties.Single(f => f.Name == "HasAuthorized").GetMethod;
            Console.WriteLine("[Patch][CeVIO AI.exe] CeVIO.Editor.MissionAssistant.Authorizer.HasAuthorized.get");
            ReplaceToReturnTrue(hasAuthorizedGet);

            // CeVIO.Editor.MissionAssistant.Authorizer.TryOfflineStartup
            // var tryOfflineStartupMethod = authorizerType.Methods.Single(m => m.Name == "TryOfflineStartup");

            // CeVIO.Editor.MissionAssistant.ProductLicense
            var productLicenseType = asm.MainModule.GetType("CeVIO.Editor.MissionAssistant.ProductLicense");

            // CeVIO.Editor.MissionAssistant.ProductLicense.AllowsOffline.get
            var allowsOfflineGet = productLicenseType.Properties.Single(p => p.Name == "AllowsOffline").GetMethod;
            Console.WriteLine("[Patch][CeVIO AI.exe] CeVIO.Editor.MissionAssistant.ProductLicense.AllowsOffline.get");
            ReplaceToReturnTrue(allowsOfflineGet);

            // CeVIO.Editor.MissionAssistant.ProductLicense.OfflineAcceptablePeriod
            var offlineAcceptablePeriodField = productLicenseType.Fields.Single(p => p.Name == "OfflineAcceptablePeriod");
            var productLicenseCctor = productLicenseType.Methods.Single(m => m.Name == ".cctor");
            {
                // find instruction
                Instruction targetInstruction = null;
                foreach (var instruction in productLicenseCctor.Body.Instructions)
                {
                    if (instruction.OpCode == OpCodes.Ldc_R8 && (double)instruction.Operand == 365)
                    {
                        targetInstruction = instruction;
                        break;
                    }
                }

                // replace instruction
                if (targetInstruction != null)
                {
                    Console.WriteLine("[Patch][CeVIO AI.exe] CeVIO.Editor.MissionAssistant.ProductLicense.OfflineAcceptablePeriod");

                    var processor = productLicenseCctor.Body.GetILProcessor();
                    processor.Replace(
                        productLicenseCctor.Body.Instructions.IndexOf(targetInstruction) + 1,
                        processor.Create(
                            OpCodes.Ldsfld,
                            asm.MainModule.ImportReference(typeof(TimeSpan).GetField(nameof(TimeSpan.MaxValue)))
                            )
                        );
                    processor.Remove(targetInstruction);
                }
                else
                {
                    Console.WriteLine("[Skip][CeVIO AI.exe] CeVIO.Editor.MissionAssistant.ProductLicense.OfflineAcceptablePeriod");
                }
            }

            if (dryrun)
            {
                return;
            }

            // copy backup
            MakeBackup(cevioInstallPath, "CeVIO AI.exe");

            // save
            Console.WriteLine("[Save] " + cevioExecutablePath);
            asm.Write(cevioExecutablePath);
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

        public static void BypassAuthentication(string cevioInstallPath, bool dryrun = false)
        {
            if (dryrun)
            {
                Console.WriteLine("Dryrun mode");
            }
            string[] AuthenticationMethods = {
                "CeVIO.Talk.LocalPermission.Assert",
                "CeVIO.SongEditorControl.SongEditorControl.Authentication",
            };

            foreach (var fullName in AuthenticationMethods)
            {
                // get names
                var nameParts = fullName.Split('.');
                var assemblyName = nameParts[0] + "." + nameParts[1] + ".dll";
                var typeName = nameParts[0] + "." + nameParts[1] + "." + nameParts[2];
                var methodName = nameParts[3];

                // check file exists
                var modulePath = Path.Combine(cevioInstallPath, assemblyName);
                if (!File.Exists(modulePath))
                {
                    throw new FileNotFoundException($"{modulePath} not found");
                }

                // open module
                var module = ModuleDefinition.ReadModule(modulePath);
                var type = module.GetType(typeName);
                var methodDef = type.Methods.Single(m => m.Name == methodName);

                // check patched
                if (methodDef.Body.Instructions.Count == 1 && methodDef.Body.Instructions[0].OpCode == OpCodes.Ret)
                {
                    Console.WriteLine($"[Skip][{assemblyName}] Already patched {typeName}.{methodName}");
                    continue;
                }

                Console.WriteLine($"[Patch][{assemblyName}] {typeName}.{methodName}");

                // clear method body
                ClearMethodBody(methodDef);

                if (dryrun)
                {
                    continue;
                }

                // copy backup
                MakeBackup(cevioInstallPath, assemblyName);

                // save
                Console.WriteLine("[Save] " + modulePath);
                module.Write(modulePath);
            }

            // CeVIO.Song tssinger2.TSSingerCLI..cctor
            var songModulePath = Path.Combine(cevioInstallPath, "CeVIO.Song.dll");
            var songModule = ModuleDefinition.ReadModule(songModulePath);
            var tssinger2Type = songModule.GetType("tssinger2.TSSingerCLI");
            var cctor = tssinger2Type.Methods.Single(m => m.Name == ".cctor");
            var reserveCount = 2;

            // check patched
            if (cctor.Body.Instructions.Count == reserveCount)
            {
                if (cctor.Body.Instructions[0].OpCode == OpCodes.Ldc_I4_0 &&
                    cctor.Body.Instructions[1].OpCode == OpCodes.Stsfld)
                {
                    Console.WriteLine("[Skip][CeVIO.Song.dll] Already patched tssinger2.TSSingerCLI..cctor");
                    return;
                }
            }

            Console.WriteLine("[Patch][CeVIO.Song.dll] tssinger2.TSSingerCLI..cctor");

            // clear method body, only reserve "TSSingerCLI.trigger = 0;"
            var cctorProcessor = cctor.Body.GetILProcessor();
            while (cctor.Body.Instructions.Count > reserveCount)
            {
                cctorProcessor.Remove(cctor.Body.Instructions[reserveCount]);
            }
            cctorProcessor.Emit(OpCodes.Ret);

            if (dryrun)
            {
                return;
            }

            // copy backup
            MakeBackup(cevioInstallPath, "CeVIO.Song.dll");

            // save
            Console.WriteLine("[Save] " + songModulePath);
            songModule.Write(songModulePath);
        }
    }
}
