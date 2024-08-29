using dnlib.DotNet;
using dnlib.DotNet.Emit;
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

        private static void ReplaceToReturnTrue(MethodDef method)
        {
            method.Body.Instructions.Clear();
            method.Body.Instructions.Add(OpCodes.Ldc_I4_1.ToInstruction());
            method.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
        }

        private static void ClearMethodBody(MethodDef method)
        {
            method.Body.Instructions.Clear();
            method.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
        }

        private static void MakeBackup(string cevioInstallPath, string filename)
        {
            var sourcePath = Path.Combine(cevioInstallPath, filename);
            var backupPath = Path.Combine(cevioInstallPath, filename + BACKUP_POSTFIX);

            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"[Backup] {filename} Not found");
                return;
            }

            if (File.Exists(backupPath))
            {
                Console.WriteLine("[Backup] " + backupPath + " already exists, skip backup");
                return;
            }

            Console.WriteLine("[Backup] " + filename);
            File.Copy(sourcePath, backupPath);
        }

        public static void WriteMixed(this ModuleDefMD module, string filename)
        {
            if (module.IsILOnly)
            {
                module.Write(filename);
            }
            else
            {
                module.NativeWrite(filename);
            }
        }

        public static void PatchExecutable(string cevioInstallPath, bool dryrun = false)
        {
            // read assembly
            var cevioExecutablePath = Path.Combine(cevioInstallPath, "CeVIO AI.exe");
            var module = ModuleDefMD.Load(cevioExecutablePath);

            // CeVIO.Editor.MissionAssistant.Authorizer
            var authorizerType = module.Types.Single(m => m.FullName == "CeVIO.Editor.MissionAssistant.Authorizer");

            // CeVIO.Editor.MissionAssistant.Authorizer.ForciblyAuthorize
            var forciblyAuthorizeMethod = authorizerType.Methods.Single(m => m.Name == "ForciblyAuthorize");
            {
                var instructions = forciblyAuthorizeMethod.Body.Instructions;

                // find instruction
                Instruction targetInstruction = null;
                foreach (var instruction in instructions)
                {
                    if (instruction.OpCode == OpCodes.Ldsfld)
                    {
                        var operand = (MemberRef)instruction.Operand;
                        if (operand.DeclaringType.FullName == typeof(DateTime).FullName &&
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

                    instructions[instructions.IndexOf(targetInstruction)] = OpCodes.Call.ToInstruction(
                        module.Import(typeof(DateTime).GetProperty(nameof(DateTime.Now)).GetMethod));
                }
                else
                {
                    Console.WriteLine("[Skip][CeVIO AI.exe] CeVIO.Editor.MissionAssistant.Authorizer.ForciblyAuthorize");
                }
            }

            // CeVIO.Editor.MissionAssistant.Authorizer.Authorize
            var authorizeMethod = authorizerType.Methods.Single(m => m.Name == "Authorize");
            {
                var instructions = authorizeMethod.Body.Instructions;

                Instruction targetInstruction = null;
                for (var i = 0; i < instructions.Count; i++)
                {
                    var instruction = instructions[i];

                    if (instruction.OpCode == OpCodes.Throw)
                    {
                        if (instructions[i - 2].OpCode == OpCodes.Call &&
                            instructions[i - 1].OpCode == OpCodes.Newobj)
                        {
                            var operand = (IMemberDef)instructions[i - 2].Operand;
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

                    var reserveCount = instructions.IndexOf(targetInstruction) + 1;
                    while (instructions.Count > reserveCount)
                    {
                        instructions.Remove(instructions[reserveCount]);
                    }
                    instructions.Add(OpCodes.Ldloc_0.ToInstruction());
                    instructions.Add(OpCodes.Call.ToInstruction(forciblyAuthorizeMethod));
                    instructions.Add(OpCodes.Ret.ToInstruction());

                    // add custom code
                    // nop
                    foreach (var i in new int[] { 5, 1, 0 })
                    {
                        instructions.RemoveAt(i);
                    }

                    // replace
                    instructions[14] = OpCodes.Stloc_0.ToInstruction();
                    instructions[16] = OpCodes.Ldnull.ToInstruction();
                    instructions.Insert(17, OpCodes.Ceq.ToInstruction());
                    instructions[18] = OpCodes.Brfalse_S.ToInstruction(authorizeMethod.Body.Instructions[22]);

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
            var productLicenseType = module.Types.Single(t => t.FullName == "CeVIO.Editor.MissionAssistant.ProductLicense");

            // CeVIO.Editor.MissionAssistant.ProductLicense.AllowsOffline.get
            var allowsOfflineGet = productLicenseType.Properties.Single(p => p.Name == "AllowsOffline").GetMethod;
            Console.WriteLine("[Patch][CeVIO AI.exe] CeVIO.Editor.MissionAssistant.ProductLicense.AllowsOffline.get");
            ReplaceToReturnTrue(allowsOfflineGet);

            // CeVIO.Editor.MissionAssistant.ProductLicense.OfflineAcceptablePeriod
            var offlineAcceptablePeriodField = productLicenseType.Fields.Single(p => p.Name == "OfflineAcceptablePeriod");
            var productLicenseCctor = productLicenseType.Methods.Single(m => m.Name == ".cctor");
            {
                var instructions = productLicenseCctor.Body.Instructions;

                // find instruction
                Instruction targetInstruction = null;
                foreach (var instruction in instructions)
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

                    instructions[instructions.IndexOf(targetInstruction) + 1] = 
                        OpCodes.Ldsfld.ToInstruction(
                            module.Import(typeof(TimeSpan).GetField(nameof(TimeSpan.MaxValue)))
                            );
                    instructions.Remove(targetInstruction);
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
            module.Write(cevioExecutablePath);
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
                var module = ModuleDefMD.Load(modulePath);
                var type = module.Types.Single(t => t.FullName == typeName);
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
                module.WriteMixed(modulePath);
            }

            // CeVIO.Song tssinger2.TSSingerCLI..cctor
            {
                var modulePath = Path.Combine(cevioInstallPath, "CeVIO.Song.dll");
                var module = ModuleDefMD.Load(modulePath);
                var type = module.Types.Single(t => t.FullName == "tssinger2.TSSingerCLI");
                var cctor = type.Methods.Single(m => m.Name == ".cctor");
                var reserveCount = 2;

                var instructions = cctor.Body.Instructions;

                // check patched
                if (instructions.Count == reserveCount)
                {
                    if (instructions[0].OpCode == OpCodes.Ldc_I4_0 &&
                        instructions[1].OpCode == OpCodes.Stsfld)
                    {
                        Console.WriteLine("[Skip][CeVIO.Song.dll] Already patched tssinger2.TSSingerCLI..cctor");
                        return;
                    }
                }

                Console.WriteLine("[Patch][CeVIO.Song.dll] tssinger2.TSSingerCLI..cctor");

                // clear method body, only reserve "TSSingerCLI.trigger = 0;"
                while (instructions.Count > reserveCount)
                {
                    instructions.Remove(instructions[reserveCount]);
                }
                instructions.Add(OpCodes.Ret.ToInstruction());

                if (dryrun)
                {
                    return;
                }

                // copy backup
                MakeBackup(cevioInstallPath, "CeVIO.Song.dll");

                // save
                Console.WriteLine("[Save] " + modulePath);
                module.WriteMixed(modulePath);
            }
        }
    }
}
