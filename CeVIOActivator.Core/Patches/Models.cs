using dnlib.DotNet;

namespace CeVIOActivator.Core.Patches
{
    /// <summary>
    /// Define a basic patch for CeVIO, do not implement it directly
    /// </summary>
    interface ICeVIOPatch
    {
        /// <summary>
        /// Supported CeVIO version
        /// </summary>
        CeVIOVersion TargetVersion { get; }
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
    interface ICeVIOMethodPatch : ICeVIOPatch
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
    interface ICeVIOPropertyPatch : ICeVIOPatch
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
}
