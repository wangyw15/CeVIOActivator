using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace CeVIOActivator.Core
{
    internal static class MethodDefExtensions
    {
        public static bool AlreadyReturnBool(this MethodDef method, bool returnValue)
        {
            var targetOpcode = returnValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
            return method.Body.Instructions.Count == 2 &&
                method.Body.Instructions[0].OpCode == targetOpcode &&
                method.Body.Instructions[1].OpCode == OpCodes.Ret;
        }

        public static void ReturnBool(this MethodDef method, bool returnValue)
        {
            var targetOpcode = returnValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;

            method.Body.Instructions.Clear();
            if (method.Body.HasExceptionHandlers)
            {
                method.Body.ExceptionHandlers.Clear();
            }

            method.Body.Instructions.Add(targetOpcode.ToInstruction());
            method.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
        }

        public static bool AlreadyClearBody(this MethodDef method)
        {
            var cleared = true;
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Nop)
                {
                    continue;
                }

                if (instruction.OpCode != OpCodes.Ret)
                {
                    cleared = false;
                    break;
                }
            }
            return cleared;
        }

        public static void ClearBody(this MethodDef method)
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
            Console.WriteLine("Saving: " + filename);
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
