using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class RoadShaderResources : ShaderSet
    {
        public readonly Pipeline Pipeline;

        public RoadShaderResources(ShaderSetStore store)
            : base(store, "Road", RoadVertex.VertexDescriptor)
        {
            Pipeline = AddDisposable(
                store.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        BlendStateDescription.SingleAlphaBlend,
                        DepthStencilStateDescription.DepthOnlyLessEqualRead,
                        RasterizerStateDescriptionUtility.CullNoneSolid, // TODO
                        PrimitiveTopology.TriangleList,
                        Description,
                        ResourceLayouts,
                        store.OutputDescription)));
        }

        public ResourceSet CreateMaterialResourceSet(Texture texture)
        {
            // TODO: Cache these.

            return GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    MaterialResourceLayout,
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
