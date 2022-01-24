using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Rendering;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class RoadShaderResources : ShaderResourcesBase
    {
        private readonly ResourceLayout _materialResourceLayout;

        public readonly Pipeline Pipeline;

        public RoadShaderResources(GraphicsDevice graphicsDevice, GlobalShaderResources globalShaderResources)
            : base(
                graphicsDevice,
                "Road",
                RoadVertex.VertexDescriptor)
        {
            _materialResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment))));

            var resourceLayouts = new[]
            {
                globalShaderResources.GlobalConstantsResourceLayout,
                globalShaderResources.ForwardPassResourceLayout,
                _materialResourceLayout,
            };

            Pipeline = AddDisposable(graphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    BlendStateDescription.SingleAlphaBlend,
                    DepthStencilStateDescription.DepthOnlyLessEqualRead,
                    RasterizerStateDescriptionUtility.CullNoneSolid, // TODO
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
        public struct RoadVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 UV;

            public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
                new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("NORMAL", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
        }
    }
}
