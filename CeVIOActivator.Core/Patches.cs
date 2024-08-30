using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;

namespace CeVIOActivator.Core
{
    /// <summary>
    /// Define a basic patch for CeVIO, do not implement it directly
    /// </summary>
    internal interface ICeVIOPatch
    {
        /// <summary>
        /// Assembly name
        /// </summary>
        string TargetAssembly { get; }

        /// <summary>
        /// Type full name
        /// </summary>
        string TargetType { get; }
    }

    /// <summary>
    /// Define a patch of method for CeVIO
    /// </summary>
    internal interface ICeVIOMethodPatch : ICeVIOPatch
    {
        /// <summary>
        /// Method name
        /// </summary>
        string TargetMethod { get; }

        /// <summary>
        /// Check if the method is already patched
        /// </summary>
        /// <param name="method">Target method</param>
        /// <returns>If the method is already patched</returns>
        bool AlreadyPatched(MethodDef method);

        /// <summary>
        /// Patch the method, will only be called if AlreadyPatched returns false
        /// </summary>
        /// <param name="method">Method to be patched</param>
        void Patch(MethodDef method);
    }

    /// <summary>
    /// Define a patch of property for CeVIO
    /// </summary>
    internal interface ICeVIOPropertyPatch : ICeVIOPatch
    {
        /// <summary>
        /// Property name
        /// </summary>
        string TargetProperty { get; }

        /// <summary>
        /// Check if the property is already patched
        /// </summary>
        /// <param name="property">Target property</param>
        /// <returns>If the property is already patched</returns>
        bool AlreadyPatched(PropertyDef property);

        /// <summary>
        /// Patch the property, will only be called if AlreadyPatched returns false
        /// </summary>
        /// <param name="property">Property to be patched</param>
        void Patch(PropertyDef property);
    }

    internal class CeVIOExecutable_Authorizer_ForciblyAuthorize_Patch : ICeVIOMethodPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.Authorizer";

        public string TargetMethod => "ForciblyAuthorize";

        private Instruction FindTargetInstruction(MethodDef method)
        {
            var instructions = method.Body.Instructions;

            Instruction target = null;
            foreach (var instruction in instructions)
            {
                if (instruction.OpCode == OpCodes.Ldsfld)
                {
                    if (instruction.Operand is MemberRef operand &&
                        operand.DeclaringType.FullName == typeof(DateTime).FullName &&
                        operand.Name == nameof(DateTime.MaxValue))
                    {
                        target = instruction;
                        break;
                    }
                }
            }
            return target;
        }

        public bool AlreadyPatched(MethodDef method) => FindTargetInstruction(method) == null;

        public void Patch(MethodDef method)
        {
            var instructions = method.Body.Instructions;
            var target = FindTargetInstruction(method);

            if (target != null)
            {
                instructions[instructions.IndexOf(target)] = OpCodes.Call.ToInstruction(
                    method.Module.Import(typeof(DateTime).GetProperty(nameof(DateTime.Now)).GetMethod));
            }
        }
    }

    internal class CeVIOExecutable_Authorizer_Authorize_Patch : ICeVIOMethodPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.Authorizer";

        public string TargetMethod => "Authorize";

        private Instruction FindTargetInstruction(MethodDef method)
        {
            var instructions = method.Body.Instructions;

            Instruction target = null;
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
                            target = instruction;
                            break;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return target;
        }

        public bool AlreadyPatched(MethodDef method)
        {
            var target = FindTargetInstruction(method);
            var patchedMethodCount = method.Body.Instructions.IndexOf(target) + 1 + 3;
            return method.Body.Instructions.Count == patchedMethodCount;
        }

        public void Patch(MethodDef method)
        {
            var instructions = method.Body.Instructions;
            var target = FindTargetInstruction(method);

            var forciblyAuthorizeMethod = method.DeclaringType.Methods.Single(m => m.Name == "ForciblyAuthorize");

            var reserveCount = instructions.IndexOf(target) + 1;
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
            instructions[18] = OpCodes.Brfalse_S.ToInstruction(instructions[22]);

            method.Body.ExceptionHandlers.Clear();
        }
    }

    internal class CeVIOExecutable_Authorizer_HasAuthorized_Patch : ICeVIOPropertyPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.Authorizer";

        public string TargetProperty => "HasAuthorized";

        public bool AlreadyPatched(PropertyDef property) => property.GetMethod.CheckAlreadyReturnTrue();

        public void Patch(PropertyDef property) => property.GetMethod.ReplaceToReturnTrue();
    }

    internal class CeVIOExecutable_ProductLicense_AllowsOffline_Patch : ICeVIOPropertyPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLicense";

        public string TargetProperty => "AllowsOffline";

        public bool AlreadyPatched(PropertyDef property) => property.GetMethod.CheckAlreadyReturnTrue();

        public void Patch(PropertyDef property) => property.GetMethod.ReplaceToReturnTrue();
    }

    internal class CeVIOExecutable_ProductLicense_OfflineAcceptablePeriod_Patch : ICeVIOMethodPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLicense";

        public string TargetMethod => ".cctor";

        private Instruction FindTargetInstruction(MethodDef method)
        {
            var instructions = method.Body.Instructions;

            Instruction target = null;
            foreach (var instruction in instructions)
            {
                if (instruction.OpCode == OpCodes.Ldc_R8 && (double)instruction.Operand == 365)
                {
                    target = instruction;
                    break;
                }
            }
            return target;
        }

        public bool AlreadyPatched(MethodDef method) => FindTargetInstruction(method) == null;

        public void Patch(MethodDef method)
        {
            var instructions = method.Body.Instructions;

            var target = FindTargetInstruction(method);
            instructions[instructions.IndexOf(target) + 1] = OpCodes.Ldsfld.ToInstruction(
                method.Module.Import(typeof(TimeSpan).GetField(nameof(TimeSpan.MaxValue)))
                );
            instructions.Remove(target);
        }
    }

    internal class Talk_LocalPermission_Assert_Patch : ICeVIOMethodPatch
    {
        public string TargetAssembly => "CeVIO.Talk.dll";

        public string TargetType => "CeVIO.Talk.LocalPermission";

        public string TargetMethod => "Assert";

        public bool AlreadyPatched(MethodDef method) => method.CheckAlreadyClearMethodBody();

        public void Patch(MethodDef method) => method.ClearMethodBody();
    }

    internal class SongEditorControl_SongEditorControl_Authentication_Patch : ICeVIOMethodPatch
    {
        public string TargetAssembly => "CeVIO.SongEditorControl.dll";

        public string TargetType => "CeVIO.SongEditorControl.SongEditorControl";

        public string TargetMethod => "Authentication";

        public bool AlreadyPatched(MethodDef method) => method.CheckAlreadyClearMethodBody();

        public void Patch(MethodDef method) => method.ClearMethodBody();
    }

    internal class Song_tssinger2_TSSingerCLI_cctor_Patch : ICeVIOMethodPatch
    {
        public string TargetAssembly => "CeVIO.Song.dll";

        public string TargetType => "tssinger2.TSSingerCLI";

        public string TargetMethod => ".cctor";

        private const int RESERVE_COUNT = 2;

        public bool AlreadyPatched(MethodDef method)
        {
            var instructions = method.Body.Instructions;
            var targetField = method.DeclaringType.Fields.Single(f => f.Name == "trigger");

            return instructions[0].OpCode == OpCodes.Ldc_I4_0 &&
                   instructions[1].OpCode == OpCodes.Stsfld && instructions[1].Operand == targetField &&
                   instructions[2].OpCode == OpCodes.Ret;
        }

        public void Patch(MethodDef method)
        {
            var instructions = method.Body.Instructions;
            var targetField = method.DeclaringType.Fields.Single(f => f.Name == "trigger");

            instructions.Clear();
            instructions.Add(OpCodes.Ldc_I4_0.ToInstruction());
            instructions.Add(OpCodes.Stsfld.ToInstruction(targetField));
            instructions.Add(OpCodes.Ret.ToInstruction());
        }
    }
}
