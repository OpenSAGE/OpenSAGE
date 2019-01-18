using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal abstract class ShaderMaterialShaderResources : ShaderResourcesBase
    {
        public readonly Pipeline Pipeline;

        public ShaderMaterialShaderResources(GraphicsDevice graphicsDevice, string shaderName)
            : base(
                  graphicsDevice,
                  shaderName,
                  new GlobalResourceSetIndices(0u, LightingType.Object, 1u, 2u, 3u, 6u),
                  MeshShaderResources.MeshVertex.VertexDescriptors)
        {
            Pipeline = AddDisposable(graphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    BlendStateDescription.SingleDisabled,
                    DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                    PrimitiveTopology.TriangleStrip,
                    ShaderSet.Description,
                    ShaderSet.ResourceLayouts,
                    RenderPipeline.GameOutputDescription)));
        }
    }
}
