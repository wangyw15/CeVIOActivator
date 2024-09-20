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

    /// <summary>
    /// Completely clear Authorizer.Authorize
    /// </summary>
    internal class CeVIOExecutable_Authorizer_Authorize_Patch : ICeVIOMethodPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.Authorizer";

        public string TargetMethod => "Authorize";

        public bool AlreadyPatched(MethodDef method) => method.AlreadyClearBody();

        public void Patch(MethodDef method) => method.ClearBody();
    }

    /// <summary>
    /// Enable offline export
    /// </summary>
    internal class CeVIOExecutable_Authorizer_HasAuthorized_Patch : ICeVIOPropertyPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.Authorizer";

        public string TargetProperty => "HasAuthorized";

        public bool AlreadyPatched(PropertyDef property) => property.GetMethod.AlreadyReturnBool(true);

        public void Patch(PropertyDef property) => property.GetMethod.ReturnBool(true);
    }

    /// <summary>
    /// Block access to auth service
    /// </summary>
    internal class CeVIOExecutable_Authorizer_ServiceIsAvailable_Patch : ICeVIOPropertyPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.Authorizer";

        public string TargetProperty => "ServiceIsAvailable";

        public bool AlreadyPatched(PropertyDef property) => property.GetMethod.AlreadyReturnBool(false);

        public void Patch(PropertyDef property) => property.GetMethod.ReturnBool(false);
    }

    /// <summary>
    /// Set the product license to allow offline
    /// </summary>
    internal class CeVIOExecutable_ProductLicense_AllowsOffline_Patch : ICeVIOPropertyPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLicense";

        public string TargetProperty => "AllowsOffline";

        public bool AlreadyPatched(PropertyDef property) => property.GetMethod.AlreadyReturnBool(true);

        public void Patch(PropertyDef property) => property.GetMethod.ReturnBool(true);
    }

    /// <summary>
    /// Set the product license to activated
    /// </summary>
    internal class CeVIOExecutable_ProductLicense_IsActivated_Patch : ICeVIOPropertyPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLicense";

        public string TargetProperty => "IsActivated";

        public bool AlreadyPatched(PropertyDef property) => property.GetMethod.AlreadyReturnBool(true);

        public void Patch(PropertyDef property) => property.GetMethod.ReturnBool(true);
    }

    /// <summary>
    /// Set the product license to authorized
    /// </summary>
    internal class CeVIOExecutable_ProductLicense_IsAuthorized_Patch : ICeVIOPropertyPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLicense";

        public string TargetProperty => "IsAuthorized";

        public bool AlreadyPatched(PropertyDef property) => property.GetMethod.AlreadyReturnBool(true);

        public void Patch(PropertyDef property) => property.GetMethod.ReturnBool(true);
    }

    /// <summary>
    /// ProductLibrary.IsAvailableFeature always returns true
    /// </summary>
    internal class CeVIOExecutable_ProductLibrary_IsAvailableFeature_Patch : ICeVIOMethodPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLibrary";

        public string TargetMethod => "IsAvailableFeature";

        public bool AlreadyPatched(MethodDef method) => method.AlreadyReturnBool(true);

        public void Patch(MethodDef method) => method.ReturnBool(true);
    }

    /// <summary>
    /// ProductLibrary.SongIsAvailable always true
    /// </summary>
    internal class CeVIOExecutable_ProductLibrary_SongIsAvailable_Patch : ICeVIOPropertyPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLibrary";

        public string TargetProperty => "SongIsAvailable";

        public bool AlreadyPatched(PropertyDef property)
        {
            var targetProperty = property.DeclaringType.Properties.Single(p => p.Name == "SongIsExisting");
            return property.GetMethod.AlreadyReturnProperty(targetProperty);
        }

        public void Patch(PropertyDef property)
        {
            var targetProperty = property.DeclaringType.Properties.Single(p => p.Name == "SongIsExisting");
            property.GetMethod.ReturnProperty(targetProperty);
        }
    }

    /// <summary>
    /// ProductLibrary.TalkIsAvailable always true
    /// </summary>
    internal class CeVIOExecutable_ProductLibrary_TalkIsAvailable_Patch : ICeVIOPropertyPatch
    {
        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLibrary";

        public string TargetProperty => "TalkIsAvailable";

        public bool AlreadyPatched(PropertyDef property)
        {
            var instructions = property.GetMethod.Body.Instructions;
            return instructions[0].OpCode == OpCodes.Ldarg_0 &&
                instructions[1].OpCode == OpCodes.Call &&
                (instructions[1].Operand as MethodDef).Name == "get_TalkIsExisting" &&
                instructions[2].OpCode == OpCodes.Ret;
        }

        public void Patch(PropertyDef property)
        {
            var availableGetter = property.DeclaringType.Properties.Single(p => p.Name == "TalkIsExisting").GetMethod;
            var method = property.GetMethod;

            var instructions = method.Body.Instructions;
            instructions.Clear();
            if (method.Body.HasExceptionHandlers)
            {
                method.Body.ExceptionHandlers.Clear();
            }

            instructions.Add(OpCodes.Ldarg_0.ToInstruction());
            instructions.Add(OpCodes.Call.ToInstruction(availableGetter));
            instructions.Add(OpCodes.Ret.ToInstruction());
        }
    }

    /// <summary>
    /// Bypass hash check for the main executable
    /// </summary>
    internal class Talk_LocalPermission_Assert_Patch : ICeVIOMethodPatch
    {
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
        public string TargetAssembly => "CeVIO.SongEditorControl.dll";

        public string TargetType => "CeVIO.SongEditorControl.SongEditorControl";

        public string TargetMethod => "Authentication";

        public bool AlreadyPatched(MethodDef method) => method.AlreadyClearBody();

        public void Patch(MethodDef method) => method.ClearBody();
    }

    /// <summary>
    /// Bypass hash check for the main executable
    /// </summary>
    internal class Song_tssinger2_TSSingerCLI_cctor_Patch : ICeVIOMethodPatch
    {
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
}
