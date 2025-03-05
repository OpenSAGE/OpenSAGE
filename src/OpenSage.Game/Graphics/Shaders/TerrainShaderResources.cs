using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Mathematics;
using OpenSage.Graphics.Rendering;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders;

internal sealed class TerrainShaderResources : ShaderSetBase
{
    public readonly Pipeline Pipeline;

    public TerrainShaderResources(ShaderSetStore store)
        : base(store, "Terrain", TerrainVertex.VertexDescriptor)
    {
        Pipeline = AddDisposable(store.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
            new GraphicsPipelineDescription(
                BlendStateDescription.SingleDisabled,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                PrimitiveTopology.TriangleList, // TODO: Use triangle strip
                Description,
                ResourceLayouts,
                RenderPipeline.GameOutputDescription)));
    }

    public ResourceSet CreateMaterialResourceSet(
        DeviceBuffer materialConstantsBuffer,
        Texture tileDataTexture,
        DeviceBuffer cliffDetailsBuffer,
        DeviceBuffer textureDetailsBuffer,
        Texture textureArray,
        Texture macroTexture,
        Texture casuticsTextures)
    {
        return GraphicsDevice.ResourceFactory.CreateResourceSet(
            new ResourceSetDescription(
                MaterialResourceLayout,
                materialConstantsBuffer,
                tileDataTexture,
                cliffDetailsBuffer,
                textureDetailsBuffer,
                textureArray,
                macroTexture,
                casuticsTextures,
                GraphicsDevice.Aniso4xSampler));
    }

    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public struct TerrainMaterialConstants
    {
        [FieldOffset(0)]
        public Vector2 MapBorderWidth;

        [FieldOffset(8)]
        public Vector2 MapSize;

        [FieldOffset(16)]
        public Bool32 IsMacroTextureStretched;

        [FieldOffset(20)]
        public int CausticTextureIndex;
    }

    public struct TextureInfo
    {
        public uint TextureIndex;
        public uint CellSize;
        public Vector2 _Padding;
    }

    public struct CliffInfo
    {
        public const int Size = sizeof(float) * 8;

        public Vector2 BottomLeftUV;
        public Vector2 BottomRightUV;
        public Vector2 TopRightUV;
        public Vector2 TopLeftUV;
    }

    internal struct TerrainVertex
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
