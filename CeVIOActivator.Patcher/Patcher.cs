using HarmonyLib;
using System;
using System.IO;
using System.Reflection;

namespace CeVIOActivator.Patcher
{
    public static class Patcher
    {
        static Patcher()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        public static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var file = Path.Combine(location, args.Name.Split(',')[0] + ".dll");
                if (!File.Exists(file))
                {
                    file = Path.Combine(location, args.Name.Split(',')[0] + ".exe");
                }
                return Assembly.LoadFrom(file);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [DllExport]
        public static int Patch()
        {
            var productName = Assembly.GetEntryAssembly().GetName().FullName;
            productName = productName.Split(',')[0];

            try
            {
                var harmony = new Harmony("cevio.wangyw15");
                harmony.PatchAllUncategorized(Assembly.GetExecutingAssembly());
                if (productName == "CeVIO AI")
                {
                    harmony.PatchCategory(Assembly.GetExecutingAssembly(), "AI");
                }
                else
                {
                    harmony.PatchCategory(Assembly.GetExecutingAssembly(), "CS");
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                using (var writer = new StreamWriter(Path.Combine("C:/", Assembly.GetExecutingAssembly().FullName + ".log")))
                {
                    writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    writer.WriteLine(ex);
                }
#endif
                return -1;
            }

            return 771;
        }
    }

    [HarmonyPatch("CeVIO.Editor.MissionAssistant.Authorizer", "Authorize")]
    public class AuthorizePatcher
    {
        public static bool Prefix(MethodBase __originalMethod)
        {
            // in case the getter of Authorizer.HasAuthorized is inlined
            var hasAuthorized = __originalMethod.DeclaringType.GetProperty("HasAuthorized");
            if (hasAuthorized != null)
            {
                hasAuthorized.SetValue(null, true);
            }

            return false;
        }
    }

    [HarmonyPatchCategory("AI")]
    [HarmonyPatch("CeVIO.Editor.MissionAssistant.Authorizer", "HasAuthorized", MethodType.Getter)]
    public class HasAuthorizedPatcher
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch("CeVIO.Editor.MissionAssistant.Authorizer", "IsAvailableFeature")]
    public class Authorizer_IsAvailableFeaturePatcher
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch("CeVIO.Editor.MissionAssistant.ProductLicense", "AllowsOffline", MethodType.Getter)]
    public class AllowsOfflinePatcher
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch("CeVIO.Editor.MissionAssistant.ProductLicense", "IsActivated", MethodType.Getter)]
    public class IsActivatedPatcher
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch("CeVIO.Editor.MissionAssistant.ProductLicense", "IsAuthorized", MethodType.Getter)]
    public class IsAuthorizedPatcher
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch("CeVIO.Editor.MissionAssistant.ProductLibrary", "IsAvailableFeature")]
    public class ProductLibrary_IsAvailableFeaturePatcher
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch("CeVIO.Editor.MissionAssistant.ProductDataBase", "IsAvailable", MethodType.Getter)]
    public class IsAvailablePatcher
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch("CeVIO.Editor.MissionAssistant.ProductDataBase", "IsAvailable0", MethodType.Getter)]
    public class IsAvailable0Patcher
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch("CeVIO.Editor.MissionAssistant.ProductLibrary", "SongIsAvailable", MethodType.Getter)]
    public class SongIsAvailablePatcher
    {
        public static bool Prefix(ref dynamic __instance, ref bool __result)
        {
            __result = __instance.SongIsExisting;
            return false;
        }
    }

    [HarmonyPatch("CeVIO.Editor.MissionAssistant.ProductLibrary", "TalkIsAvailable", MethodType.Getter)]
    public class TalkIsAvailablePatcher
    {
        public static bool Prefix(ref dynamic __instance, ref bool __result)
        {
            __result = __instance.TalkIsExisting;
            return false;
        }
    }

    #region Block Internet access
    [HarmonyPatch("CeVIO.Editor.MissionAssistant.Authorizer", "ServiceIsAvailable", MethodType.Getter)]
    public class ServiceIsAvailablePatcher
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    [HarmonyPatch("CeVIO.Editor.Adapters.MainWindowCircuit", "ThisVersionIsAvailable")]
    public class ThisVersionIsAvailablePatcher
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch("CeVIO.Editor.MissionAssistant.AmbientSetting", "CheckForUpdatesOnStartUp", MethodType.Getter)]
    public class CheckForUpdatesOnStartUpPatcher
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    //[HarmonyPatch("CeVIO.Editor.MissionAssistant.ProductLibrary", "GetAdditionalVocalSourceSettingsMaster")]
    //public class GetAdditionalVocalSourceSettingsMasterPatcher
    //{
    //    public static bool Prefix()
    //    {
    //        return false;
    //    }
    //}
    #endregion
}
