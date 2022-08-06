using OpenSage.Core.Graphics;
using OpenSage.Graphics.Shaders;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Content
{
    public sealed class GraphicsLoadContext
    {
        public GraphicsDeviceManager GraphicsDeviceManager { get; }
        public GraphicsDevice GraphicsDevice { get; }
        internal ShaderResourceManager ShaderResources { get; }
        public ShaderSetStore ShaderSetStore { get; }

        internal GraphicsLoadContext(
            GraphicsDeviceManager graphicsDeviceManager,
            ShaderResourceManager shaderResources,
            ShaderSetStore shaderSetStore)
        {
            GraphicsDeviceManager = graphicsDeviceManager;
            GraphicsDevice = graphicsDeviceManager.GraphicsDevice;
            ShaderResources = shaderResources;
            ShaderSetStore = shaderSetStore;
        }
    }
}
