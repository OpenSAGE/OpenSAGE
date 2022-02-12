using OpenSage.Graphics.Rendering.Shadows;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class MeshDepthShaderResources : ShaderSetBase
    {
        public readonly Material Material;

        public MeshDepthShaderResources(
            ShaderSetStore store)
            : base(store, "MeshDepth", MeshShaderResources.MeshVertex.VertexDescriptors)
        {
            var depthRasterizerState = RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise;
            depthRasterizerState.DepthClipEnabled = false;
            depthRasterizerState.ScissorTestEnabled = false;

            var pipeline = AddDisposable(
                GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        BlendStateDescription.SingleDisabled,
                        DepthStencilStateDescription.DepthOnlyLessEqual,
                        depthRasterizerState,
                        PrimitiveTopology.TriangleList,
                        Description,
                        ResourceLayouts,
                        ShadowData.DepthPassDescription)));

            Material = AddDisposable(
                new Material(
                    this,
                    pipeline,
                    null));
        }
    }
}
