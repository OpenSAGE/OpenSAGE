using OpenSage.Graphics.Rendering.Shadows;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class MeshDepthShaderResources : ShaderResourcesBase
    {
        public readonly Pipeline Pipeline;

        public MeshDepthShaderResources(
            GraphicsDevice graphicsDevice,
            GlobalShaderResources globalShaderResources,
            MeshShaderResources meshShaderResources)
            : base(
                graphicsDevice,
                "MeshDepth",
                new GlobalResourceSetIndices(0u, LightingType.None, null, null, null, 2u),
                MeshShaderResources.MeshVertex.VertexDescriptors)
        {
            var depthRasterizerState = RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise;
            depthRasterizerState.DepthClipEnabled = false;
            depthRasterizerState.ScissorTestEnabled = false;

            var resourceLayouts = new[]
            {
                globalShaderResources.GlobalConstantsResourceLayout,
                meshShaderResources.MeshConstantsResourceLayout,
                meshShaderResources.RenderItemConstantsResourceLayout,
                meshShaderResources.SkinningResourceLayout
            };

            Pipeline = AddDisposable(graphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    BlendStateDescription.SingleDisabled,
                    DepthStencilStateDescription.DepthOnlyLessEqual,
                    depthRasterizerState,
                    PrimitiveTopology.TriangleList,
                    ShaderSet.Description,
                    resourceLayouts,
                    ShadowData.DepthPassDescription)));
        }
    }
}
