using System;
using OpenSage.Graphics.Rendering.Shadows;
using OpenSage.Rendering;
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
                MeshShaderResources.MeshVertex.VertexDescriptors)
        {
            var depthRasterizerState = RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise;
            depthRasterizerState.DepthClipEnabled = false;
            depthRasterizerState.ScissorTestEnabled = false;

            var emptyResourceLayout = AddDisposable(
                graphicsDevice.ResourceFactory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        Array.Empty<ResourceLayoutElementDescription>())));

            var resourceLayouts = new[]
            {
                globalShaderResources.GlobalConstantsResourceLayout,
                emptyResourceLayout,
                emptyResourceLayout,
                meshShaderResources.RenderItemConstantsResourceLayout,
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
