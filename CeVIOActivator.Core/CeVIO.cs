using System;
using System.Collections.Generic;
using System.Reflection;

namespace CeVIOActivator.Core
{
    public interface ICeVIO
    {
        Assembly CeVIOAssembly { get; set; }
    }

    public class App : MarshalByRefObject, ICeVIO
    {
        private Assembly _assembly;

        private Type _type = null;

        public Assembly CeVIOAssembly
        {
            get
            {
                return _assembly;
            }
            set
            {
                _assembly = value;
                _type = _assembly.GetType("CeVIO.Editor.App");
            } 
        }
        
        public string CommonName
        {
            get
            {
                var name = _type.GetField("CommonName");
                return name.GetValue(null) as string;
            }
        }
    }
    
    public class ProductLicense : MarshalByRefObject, ICeVIO
    {
        private Assembly _assembly;

        private Type _type = null;

        public Assembly CeVIOAssembly
        {
            get
            {
                return _assembly;
            }
            set
            {
                _assembly = value;
                _type = _assembly.GetType("CeVIO.Editor.MissionAssistant.ProductLicense");
            }
        }
        
        public DateTime DescrambleDateTime(byte[] value)
        {
            var method = _type.GetMethod("DescrambleDateTime", BindingFlags.Static | BindingFlags.NonPublic);
            return (DateTime)method.Invoke(null, new object[] { value });
        }

        public byte[] ScrambleDateTime(DateTime value)
        {
            var method = _type.GetMethod("ScrambleDateTime", BindingFlags.Static | BindingFlags.NonPublic);
            return (byte[])method.Invoke(null, new object[] { value });
        }

        public TimeSpan OfflineAcceptablePeriod
        {
            get
            {
                var span = _type.GetField("OfflineAcceptablePeriod");
                return (TimeSpan)span.GetValue(null);
            }
        }
    }

    public class LicenseSummary : MarshalByRefObject, ICeVIO
    {
        private Assembly _assembly;

        private Type _type = null;

        public Assembly CeVIOAssembly
        {
            get
            {
                return _assembly;
            }
            set
            {
                _assembly = value;
                _type = _assembly.GetType("CeVIO.Editor.MissionAssistant.LicenseSummary");
            }
        }

        public void Save()
        {
            var save = _type.GetMethod("Save", BindingFlags.Static | BindingFlags.Public);
            save.Invoke(null, null);
        }

        public void AddFeature(Feature feature)
        {
            var add = _type.GetMethod("AddFeature", BindingFlags.NonPublic | BindingFlags.Static);
            add.Invoke(null, new object[] { feature });
        }

        public void AddPackageCodes(IEnumerable<Guid> codes)
        {
            var add = _type.GetMethod("AddPackageCodes", new Type[] { typeof(IEnumerable<Guid>) });
            add.Invoke(null, new object[] { codes });
        }

        public string KeyPath
        {
            get
            {
                var keyPath = _type.GetField("keyPath");
                return keyPath.GetValue(null) as string;
            }
        }

        public Type PackageUnit
        {
            get
            {
                return _type.GetNestedType("PackageUnit", BindingFlags.NonPublic);
            }
        }
    }

    public enum Feature : uint
    {
        Unknown = 0U,
        Talking = 1U,
        Singing = 2U,
        Full = 3U
    }
}
