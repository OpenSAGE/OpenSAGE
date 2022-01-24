using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Rendering;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class WaterShaderResources : ShaderSet
    {
        public readonly ResourceLayout WaterResourceLayout;

        public readonly Pipeline Pipeline;

        public WaterShaderResources(ShaderSetStore store)
            : base(store, "Water", WaterVertex.VertexDescriptor)
        {
            WaterResourceLayout = AddDisposable(store.GraphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("WaterConstantsPS", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("WaterTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("BumpTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("WaterSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("ReflectionMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("ReflectionMapSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RefractionMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RefractionMapSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RefractionDepthMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment))));

            Pipeline = AddDisposable(store.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    BlendStateDescription.SingleAlphaBlend,
                    DepthStencilStateDescription.DepthOnlyLessEqualRead,
                    RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                    PrimitiveTopology.TriangleList,
                    Description,
                    ResourceLayouts,
                    RenderPipeline.GameOutputDescription)));
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
