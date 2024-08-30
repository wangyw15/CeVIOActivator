using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace CeVIOActivator.Libs
{
    public class Activator : MarshalByRefObject
    {
        public string ActivationKey { get; set; } = "00000-00000-00000-00000";
        
        private readonly byte[] _EmptyData = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        private App _app;
        private LicenseSummary _licenseSummary;
        private ProductLicense _productLicense;

        public void Initialize(CeVIOAssemblyManager manager)
        {
            _app = manager.CreateCeVIOInstance<App>();
            _productLicense = manager.CreateCeVIOInstance<ProductLicense>();
            _licenseSummary = manager.CreateCeVIOInstance<LicenseSummary>();
        }

        public void ActivateProducts(DateTime? expire = null)
        {
            var packageCodes = GetPackageCodes();
            var mainPackageCode = GetCeVIOProductCode();

            foreach (var code in packageCodes)
            {
                if (code.ToString() == mainPackageCode)
                {
                    WriteLicenseData($"{_licenseSummary.KeyPath}\\Creative Studio\\Product", expire);
                }
                else
                {
                    WriteLicenseData(_licenseSummary.KeyPath + "\\Product\\{" + code.ToString().ToUpper() + "}", expire);
                }
            }
        }

        public void WriteLicenseData(string keyPath, DateTime? expire = null)
        {
            using (var registryKey = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                var data = _productLicense.ScrambleDateTime(expire ?? DateTime.MaxValue);
                registryKey.SetValue(null, _EmptyData);
                registryKey.SetValue("ProductKey", ActivationKey);
                registryKey.SetValue("License", data);
                registryKey.SetValue("Registration", data);
            }
        }
        
        public IEnumerable<Guid> GetPackageCodes()
        {
            using (var registry = Registry.LocalMachine.OpenSubKey($"{_licenseSummary.KeyPath}\\Product"))
            {
                if (registry == null)
                {
                    yield break;
                }
                foreach (var x in registry.GetSubKeyNames())
                {
                    yield return new Guid(x.Split(' ')[0]);
                }
            }
        }

        public void GenerateLicenseSummary()
        {
            _licenseSummary.AddFeature(Feature.Full);
            _licenseSummary.AddPackageCodes(GetPackageCodes());
            _licenseSummary.Save();
        }

        public string GetCeVIOProductCode()
        {
            using (var registry = Registry.LocalMachine.OpenSubKey($"{_licenseSummary.KeyPath}\\Product"))
            {
                foreach(var subKey in registry.GetSubKeyNames())
                {
                    var subRegistry = registry.OpenSubKey(subKey);
                    if (subRegistry.GetValue("ProductName") as string == _app.CommonName)
                    {
                        return new Guid(subKey.Split(' ')[0]).ToString();
                    }
                }
                return "";
            }
        }
    }
}
