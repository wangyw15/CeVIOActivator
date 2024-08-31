using Microsoft.Win32;
using System.IO;

namespace CeVIOActivator.Core
{
    public class CeVIOHelper
    {
        /// <summary>
        /// Get the installation folder of CeVIO AI
        /// </summary>
        /// <returns>The installation folder of CeVIO AI</returns>
        public static string GetCeVIOAIInstallFolder()
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

        /// <summary>
        /// Get the executable path of CeVIO AI
        /// </summary>
        /// <returns>The executable path of CeVIO AI</returns>
        public static string GetCeVIOAIExecutable()
        {
            var folder = GetCeVIOAIInstallFolder();

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
