using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class WaterShaderResources : ShaderResourcesBase
    {
        private readonly ResourceLayout _materialResourceLayout;

        public readonly Pipeline Pipeline;

        public WaterShaderResources(
            GraphicsDevice graphicsDevice,
            GlobalShaderResources globalShaderResources)
            : base(
                graphicsDevice,
                "Water",
                new GlobalResourceSetIndices(0u, LightingType.Terrain, 1u, 2u, 3u, null),
                WaterVertex.VertexDescriptor)
        {
            _materialResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("WaterTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment))));

            var resourceLayouts = new[]
            {
                globalShaderResources.GlobalConstantsResourceLayout,
                globalShaderResources.GlobalLightingConstantsResourceLayout,
                globalShaderResources.GlobalCloudResourceLayout,
                globalShaderResources.GlobalShadowResourceLayout,
                _materialResourceLayout
            };

            Pipeline = AddDisposable(graphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    BlendStateDescription.SingleAlphaBlend,
                    DepthStencilStateDescription.DepthOnlyLessEqualRead,
                    RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                    PrimitiveTopology.TriangleList,
                    ShaderSet.Description,
                    resourceLayouts,
                    RenderPipeline.GameOutputDescription)));
        }

        public ResourceSet CreateMaterialResourceSet(Texture texture)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    _materialResourceLayout,
                    texture,
                    GraphicsDevice.Aniso4xSampler));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WaterVertex
        {
            public Vector3 Position;

            public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
                new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
        }
    }
}
