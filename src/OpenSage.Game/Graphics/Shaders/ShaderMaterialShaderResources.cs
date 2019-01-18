using System;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal abstract class ShaderMaterialShaderResources : ShaderResourcesBase
    {
        public readonly Pipeline Pipeline;

        protected ShaderMaterialShaderResources(
            GraphicsDevice graphicsDevice,
            GlobalShaderResources globalShaderResources,
            MeshShaderResources meshShaderResources,
            string shaderName,
            Func<GraphicsDevice, ResourceLayout> createMaterialResourceLayout)
            : base(
                  graphicsDevice,
                  shaderName,
                  new GlobalResourceSetIndices(0u, LightingType.Object, 1u, 2u, 3u, 6u),
                  MeshShaderResources.MeshVertex.VertexDescriptors)
        {
            var materialResourceLayout = AddDisposable(createMaterialResourceLayout(graphicsDevice));

            var resourceLayouts = meshShaderResources.CreateResourceLayouts(
                globalShaderResources,
                materialResourceLayout);

            Pipeline = AddDisposable(graphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    BlendStateDescription.SingleDisabled,
                    DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                    PrimitiveTopology.TriangleStrip,
                    ShaderSet.Description,
                    resourceLayouts,
                    RenderPipeline.GameOutputDescription)));
        }
    }
}
