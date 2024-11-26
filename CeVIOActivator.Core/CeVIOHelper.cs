using Microsoft.Win32;
using System.IO;

namespace CeVIOActivator.Core
{
    public class CeVIOHelper
    {
        /// <summary>
        /// Get the installation folder of CeVIO
        /// </summary>
        /// <returns>The installation folder of CeVIO</returns>
        public static string GetCeVIOAIInstallFolder(CeVIOVersion version)
        {
            if (version == CeVIOVersion.AI)
            {
                using (var reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\CeVIO_NV\\Subject\\Editor\\x64"))
                {
                    if (reg == null)
                    {
                        return null;
                    }
                    return reg.GetValue("InstallFolder") as string;
                }
            }
            else if (version == CeVIOVersion.CS7)
            {
                using (var reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\CeVIO\\Subject\\Editor\\x64"))
                {
                    if (reg == null)
                    {
                        return null;
                    }
                    return reg.GetValue("InstallFolder") as string;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the executable path of CeVIO
        /// </summary>
        /// <returns>The executable path of CeVIO</returns>
        public static string GetCeVIOAIExecutable(CeVIOVersion version)
        {
            var folder = GetCeVIOAIInstallFolder(version);

            if (string.IsNullOrEmpty(folder))
            {
                return null;
            }

            foreach (var file in Directory.GetFiles(folder))
            {
                if (Path.GetExtension(file).ToLower() == ".exe" && Path.GetFileName(file).StartsWith("CeVIO"))
                {
                    return file;
                }
            }

            return null;
        }
    }
}
