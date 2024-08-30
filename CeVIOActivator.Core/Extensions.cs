using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace CeVIOActivator.Core
{
    internal static class MethodDefExtensions
    {
        public static bool CheckAlreadyReturnTrue(this MethodDef method)
        {
            return method.Body.Instructions.Count == 2 &&
                   method.Body.Instructions[0].OpCode == OpCodes.Ldc_I4_1 &&
                   method.Body.Instructions[1].OpCode == OpCodes.Ret;
        }

        public static void ReplaceToReturnTrue(this MethodDef method)
        {
            method.Body.Instructions.Clear();
            if (method.Body.HasExceptionHandlers)
            {
                method.Body.ExceptionHandlers.Clear();
            }

            method.Body.Instructions.Add(OpCodes.Ldc_I4_1.ToInstruction());
            method.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
        }

        public static bool CheckAlreadyClearMethodBody(this MethodDef method)
        {
            return method.Body.Instructions.Count == 1 &&
                   method.Body.Instructions[0].OpCode == OpCodes.Ret;
        }

        public static void ClearMethodBody(this MethodDef method)
        {
            method.Body.Instructions.Clear();
            if (method.Body.HasExceptionHandlers)
            {
                method.Body.ExceptionHandlers.Clear();
            }

            method.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
        }
    }

    internal static class ModuleDefMDExtensions
    {
        public static void Save(this ModuleDefMD module, string filename)
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
    }
}
