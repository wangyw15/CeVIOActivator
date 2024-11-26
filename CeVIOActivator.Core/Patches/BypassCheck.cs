using System.Linq;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace CeVIOActivator.Core.Patches
{
    /// <summary>
    /// Bypass hash check for the main executable
    /// </summary>
    internal class Talk_LocalPermission_Assert_Patch : ICeVIOMethodPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.AI | CeVIOVersion.CS7;

        public string TargetAssembly => "CeVIO.Talk.dll";

        public string TargetType => "CeVIO.Talk.LocalPermission";

        public string TargetMethod => "Assert";

        public bool AlreadyPatched(MethodDef method) => method.AlreadyClearBody();

        public void Patch(MethodDef method) => method.ClearBody();
    }

    /// <summary>
    /// Bypass hash check for the main executable
    /// </summary>
    internal class SongEditorControl_SongEditorControl_Authentication_Patch : ICeVIOMethodPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.AI | CeVIOVersion.CS7;

        public string TargetAssembly => "CeVIO.SongEditorControl.dll";

        public string TargetType => "CeVIO.SongEditorControl.SongEditorControl";

        public string TargetMethod => "Authentication";

        public bool AlreadyPatched(MethodDef method) => method.AlreadyClearBody();

        public void Patch(MethodDef method) => method.ClearBody();
    }

    /// <summary>
    /// Bypass hash check for the main executable for AI
    /// </summary>
    internal class Song_tssinger2_TSSingerCLI_cctor_Patch : ICeVIOMethodPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.AI;

        public string TargetAssembly => "CeVIO.Song.dll";

        public string TargetType => "tssinger2.TSSingerCLI";

        public string TargetMethod => ".cctor";

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

    /// <summary>
    /// Bypass hash check for the main executable for CS7
    /// </summary>
    internal class Song_tssinger_TSSingerCLI_cctor_Patch : ICeVIOMethodPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.CS7;

        public string TargetAssembly => "CeVIO.Song.dll";

        public string TargetType => "tssinger.TSSingerCLI";

        public string TargetMethod => ".cctor";

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
