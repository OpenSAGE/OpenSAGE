using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    /// <summary>
    /// Shader resource classes are responsible for creating pipeline and resource sets
    /// for a particular shader set.
    /// </summary>
    internal abstract class ShaderResourcesBase : DisposableBase
    {
        protected readonly GraphicsDevice GraphicsDevice;

        public readonly ShaderSet ShaderSet;

        protected ShaderResourcesBase(
            GraphicsDevice graphicsDevice,
            string shaderName,
            GlobalResourceSetIndices globalResourceSetIndices,
            params VertexLayoutDescription[] vertexDescriptors)
        {
            GraphicsDevice = graphicsDevice;

            ShaderSet = AddDisposable(new ShaderSet(
                graphicsDevice.ResourceFactory,
                shaderName,
                globalResourceSetIndices,
                vertexDescriptors));
        }
    }
}
