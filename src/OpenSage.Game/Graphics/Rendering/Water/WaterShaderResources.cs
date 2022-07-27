using System.Numerics;
using OpenSage.Graphics.Rendering;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class WaterShaderResources : ShaderSetBase
    {
        public readonly ResourceLayout WaterResourceLayout;

        public readonly Pipeline Pipeline;

        public WaterShaderResources(ShaderSetStore store)
            : base(store, "Water", WaterVertex.VertexDescriptor)
        {
            WaterResourceLayout = ResourceLayouts[2];

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

        public struct WaterVertex
        {
            public Vector3 Position;

            public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
                new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
        }
    }
}
