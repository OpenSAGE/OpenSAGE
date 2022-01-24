using System;
using OpenSage.Graphics.Rendering.Shadows;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class MeshDepthShaderResources : ShaderSet
    {
        public readonly Pipeline Pipeline;

        public MeshDepthShaderResources(
            ShaderSetStore store)
            : base(store, "MeshDepth", MeshShaderResources.MeshVertex.VertexDescriptors)
        {
            var depthRasterizerState = RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise;
            depthRasterizerState.DepthClipEnabled = false;
            depthRasterizerState.ScissorTestEnabled = false;

            //var emptyResourceLayout = AddDisposable(
            //    graphicsDevice.ResourceFactory.CreateResourceLayout(
            //        new ResourceLayoutDescription(
            //            Array.Empty<ResourceLayoutElementDescription>())));

            Pipeline = AddDisposable(
                GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        BlendStateDescription.SingleDisabled,
                        DepthStencilStateDescription.DepthOnlyLessEqual,
                        depthRasterizerState,
                        PrimitiveTopology.TriangleList,
                        Description,
                        ResourceLayouts,
                        ShadowData.DepthPassDescription)));
        }
    }
}
