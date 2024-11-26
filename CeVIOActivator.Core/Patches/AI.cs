using System.Linq;

using dnlib.DotNet;

namespace CeVIOActivator.Core.Patches
{
    /// <summary>
    /// Completely clear Authorizer.Authorize for AI
    /// </summary>
    internal class CeVIO_AI_Executable_Authorizer_Authorize_Patch : ICeVIOMethodPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.AI;

        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.Authorizer";

        public string TargetMethod => "Authorize";

        public bool AlreadyPatched(MethodDef method) => method.AlreadyClearBody();

        public void Patch(MethodDef method) => method.ClearBody();
    }

    /// <summary>
    /// Enable offline export for AI
    /// </summary>
    internal class CeVIO_AI_Executable_Authorizer_HasAuthorized_Patch : ICeVIOPropertyPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.AI;

        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.Authorizer";

        public string TargetProperty => "HasAuthorized";

        public bool AlreadyPatched(PropertyDef property) => property.GetMethod.AlreadyReturnBool(true);

        public void Patch(PropertyDef property) => property.GetMethod.ReturnBool(true);
    }

    /// <summary>
    /// Block access to auth service for AI
    /// </summary>
    internal class CeVIO_AI_Executable_Authorizer_ServiceIsAvailable_Patch : ICeVIOPropertyPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.AI;

        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.Authorizer";

        public string TargetProperty => "ServiceIsAvailable";

        public bool AlreadyPatched(PropertyDef property) => property.GetMethod.AlreadyReturnBool(false);

        public void Patch(PropertyDef property) => property.GetMethod.ReturnBool(false);
    }

    /// <summary>
    /// Set the product license to allow offline for AI
    /// </summary>
    internal class CeVIO_AI_Executable_ProductLicense_AllowsOffline_Patch : ICeVIOPropertyPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.AI;

        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLicense";

        public string TargetProperty => "AllowsOffline";

        public bool AlreadyPatched(PropertyDef property) => property.GetMethod.AlreadyReturnBool(true);

        public void Patch(PropertyDef property) => property.GetMethod.ReturnBool(true);
    }

    /// <summary>
    /// Set the product license to activated for AI
    /// </summary>
    internal class CeVIO_AI_Executable_ProductLicense_IsActivated_Patch : ICeVIOPropertyPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.AI;

        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLicense";

        public string TargetProperty => "IsActivated";

        public bool AlreadyPatched(PropertyDef property) => property.GetMethod.AlreadyReturnBool(true);

        public void Patch(PropertyDef property) => property.GetMethod.ReturnBool(true);
    }

    /// <summary>
    /// Set the product license to authorized for AI
    /// </summary>
    internal class CeVIO_AI_Executable_ProductLicense_IsAuthorized_Patch : ICeVIOPropertyPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.AI;

        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLicense";

        public string TargetProperty => "IsAuthorized";

        public bool AlreadyPatched(PropertyDef property) => property.GetMethod.AlreadyReturnBool(true);

        public void Patch(PropertyDef property) => property.GetMethod.ReturnBool(true);
    }

    /// <summary>
    /// ProductLibrary.IsAvailableFeature always returns true for AI
    /// </summary>
    internal class CeVIO_AI_Executable_ProductLibrary_IsAvailableFeature_Patch : ICeVIOMethodPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.AI;

        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLibrary";

        public string TargetMethod => "IsAvailableFeature";

        public bool AlreadyPatched(MethodDef method) => method.AlreadyReturnBool(true);

        public void Patch(MethodDef method) => method.ReturnBool(true);
    }

    /// <summary>
    /// ProductLibrary.SongIsAvailable returns ProductLibrary.SongIsExisting for AI
    /// </summary>
    internal class CeVIO_AI_Executable_ProductLibrary_SongIsAvailable_Patch : ICeVIOPropertyPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.AI;

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
    /// ProductLibrary.TalkIsAvailable returns ProductLibrary.TalkIsExisting for AI
    /// </summary>
    internal class CeVIO_AI_Executable_ProductLibrary_TalkIsAvailable_Patch : ICeVIOPropertyPatch
    {
        public CeVIOVersion TargetVersion => CeVIOVersion.AI;

        public string TargetAssembly => "CeVIO AI.exe";

        public string TargetType => "CeVIO.Editor.MissionAssistant.ProductLibrary";

        public string TargetProperty => "TalkIsAvailable";

        public bool AlreadyPatched(PropertyDef property)
        {
            var targetProperty = property.DeclaringType.Properties.Single(p => p.Name == "TalkIsExisting");
            return property.GetMethod.AlreadyReturnProperty(targetProperty);
        }

        public void Patch(PropertyDef property)
        {
            var targetProperty = property.DeclaringType.Properties.Single(p => p.Name == "TalkIsExisting");
            property.GetMethod.ReturnProperty(targetProperty);
        }
    }
}
