using OpenSage.Graphics;
using OpenSage.Graphics.Shaders;
using Veldrid;

namespace OpenSage.Content
{
    internal sealed class GraphicsLoadContext
    {
        public GraphicsDevice GraphicsDevice { get; }
        public StandardGraphicsResources StandardGraphicsResources { get; }
        public ShaderResourceManager ShaderResources { get; }

        public GraphicsLoadContext(
            GraphicsDevice graphicsDevice,
            StandardGraphicsResources standardGraphicsResources,
            ShaderResourceManager shaderResources)
        {
            GraphicsDevice = graphicsDevice;
            StandardGraphicsResources = standardGraphicsResources;
            ShaderResources = shaderResources;
        }
    }
}
