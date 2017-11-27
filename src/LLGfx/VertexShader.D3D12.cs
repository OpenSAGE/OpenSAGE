using D3D11 = SharpDX.Direct3D11;

namespace LLGfx
{
    partial class VertexShader
    {
        internal byte[] DeviceBytecode { get; private set; }
        internal D3D11.VertexShader DeviceShader { get; private set; }

        private void PlatformConstruct(ShaderLibrary shaderLibrary, string shaderName)
        {
            DeviceBytecode = shaderLibrary.GetShader(shaderName);
            DeviceShader = AddDisposable(new D3D11.VertexShader(shaderLibrary.GraphicsDevice.Device, DeviceBytecode));
        }
    }
}
