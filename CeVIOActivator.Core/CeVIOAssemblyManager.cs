using System;
using System.IO;
using System.Reflection;

namespace CeVIOActivator.Core
{
    internal class ProxyLoader : MarshalByRefObject
    {
        public string ResolvePath { get; set; }

        public string CurrentDomainName { get => AppDomain.CurrentDomain.FriendlyName; }

        private Assembly _instance;

        public void Load(string filename)
        {
            _instance = Assembly.LoadFile(filename);
        }

        public Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            name = args.Name;
            foreach (var i in Directory.GetFiles(ResolvePath))
            {
                if (Path.GetFileNameWithoutExtension(i) == name)
                {
                    return Assembly.LoadFrom(i);
                }
            }
            return null;
        }

        public T CreateCeVIOInstance<T>() where T : ICeVIO
        {
            var type = typeof(T);
            var instance = (T)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
            instance.CeVIOAssembly = _instance;
            return instance;
        }
    }

    public class CeVIOAssemblyManager : MarshalByRefObject, IDisposable
    {
        private AppDomain _domain;

        private ProxyLoader _loader;

        public CeVIOAssemblyManager(string filename)
        {
            var directory = Path.GetDirectoryName(filename);

            var setup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                PrivateBinPath = directory,
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = directory,
            };

            _domain = AppDomain.CreateDomain("CeVIODomain", AppDomain.CurrentDomain.Evidence, setup);

            _loader = CreateInstance<ProxyLoader>();

            //_loader.ResolvePath = directory;
            //_domain.AssemblyResolve += _loader.AssemblyResolve;

            _loader.Load(filename);
        }

        public T CreateInstance<T>() where T : class
        {
            var type = typeof(T);
            var instance = (T)_domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
            return instance;
        }

        public T CreateCeVIOInstance<T>() where T : ICeVIO => _loader.CreateCeVIOInstance<T>();

        public void Dispose()
        {
            AppDomain.Unload(_domain);
        }
    }
}
