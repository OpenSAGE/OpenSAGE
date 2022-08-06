using OpenSage.Core.Graphics;
using OpenSage.Diagnostics;
using OpenSage.Rendering;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ShaderResourceManager : DisposableBase
    {
        public readonly GlobalShaderResources Global;

        public ShaderResourceManager(
            GraphicsDeviceManager graphicsDeviceManager,
            ShaderSetStore shaderSetStore)
        {
            using (GameTrace.TraceDurationEvent("ShaderResourceManager()"))
            {
                Global = AddDisposable(new GlobalShaderResources(graphicsDeviceManager.GraphicsDevice));
            }
        }
    }
}
