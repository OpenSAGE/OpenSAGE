using OpenSage.Content;
using OpenSage.Graphics.Rendering.Shadows;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class MeshDepthResourceUtility : DisposableBase
    {
        public static Pipeline CreateDepthPipeline(ContentManager contentManager, PrimitiveTopology topology)
        {
            var depthRasterizerState = RasterizerStateDescription.Default;
            depthRasterizerState.DepthClipEnabled = false;
            depthRasterizerState.ScissorTestEnabled = false;

            return contentManager.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    BlendStateDescription.SingleDisabled,
                    DepthStencilStateDescription.DepthOnlyLessEqual,
                    depthRasterizerState,
                    PrimitiveTopology.TriangleList,
                    contentManager.ShaderLibrary.MeshDepth.Description,
                    contentManager.ShaderLibrary.MeshDepth.ResourceLayouts,
                    ShadowData.DepthPassDescription));
        }
    }
}
