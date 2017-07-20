using Metal;

namespace OpenZH.Graphics
{
    partial class Shader
    {
        internal IMTLFunction DeviceFunction { get; private set; }

        private void PlatformConstruct(ShaderLibrary shaderLibrary, string shaderName)
        {
            DeviceFunction = AddDisposable(shaderLibrary.DeviceLibrary.CreateFunction(shaderName));
        }
    }
}
