using OpenSage.Graphics;
using OpenSage.Graphics.Shaders;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Content
{
    internal sealed class GraphicsLoadContext
    {
        public GraphicsDevice GraphicsDevice { get; }
        public StandardGraphicsResources StandardGraphicsResources { get; }
        public ShaderResourceManager ShaderResources { get; }
        public MaterialDefinitionStore MaterialDefinitionStore { get; }

        public GraphicsLoadContext(
            GraphicsDevice graphicsDevice,
            StandardGraphicsResources standardGraphicsResources,
            ShaderResourceManager shaderResources,
            MaterialDefinitionStore materialDefinitionStore)
        {
            GraphicsDevice = graphicsDevice;
            StandardGraphicsResources = standardGraphicsResources;
            ShaderResources = shaderResources;
            MaterialDefinitionStore = materialDefinitionStore;
        }
    }
}
