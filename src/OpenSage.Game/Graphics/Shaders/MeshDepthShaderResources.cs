using OpenSage.Graphics.Rendering.Shadows;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class MeshDepthShaderResources : ShaderResourcesBase
    {
        public readonly Pipeline TriangleStripPipeline;
        public readonly Pipeline TriangleListPipeline;

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
            var depthRasterizerState = RasterizerStateDescription.Default;
            depthRasterizerState.DepthClipEnabled = false;
            depthRasterizerState.ScissorTestEnabled = false;

            var resourceLayouts = new[]
            {
                globalShaderResources.GlobalConstantsResourceLayout,
                meshShaderResources.MeshConstantsResourceLayout,
                meshShaderResources.RenderItemConstantsResourceLayout,
                meshShaderResources.SkinningResourceLayout
            };

            Pipeline CreatePipeline(PrimitiveTopology topology)
            {
                return graphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        BlendStateDescription.SingleDisabled,
                        DepthStencilStateDescription.DepthOnlyLessEqual,
                        depthRasterizerState,
                        topology,
                        ShaderSet.Description,
                        resourceLayouts,
                        ShadowData.DepthPassDescription));
            }

            TriangleStripPipeline = AddDisposable(CreatePipeline(PrimitiveTopology.TriangleStrip));
            TriangleListPipeline = AddDisposable(CreatePipeline(PrimitiveTopology.TriangleList));
        }
    }
}
