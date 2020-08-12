using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class TerrainShaderResources : ShaderResourcesBase
    {
        private readonly Sampler _aniso4xClampSampler;

        private readonly ResourceLayout _materialResourceLayout;
        private readonly ResourceLayout _radiusCursorDecalsResourceLayout;

        public readonly Pipeline Pipeline;

        public TerrainShaderResources(
            GraphicsDevice graphicsDevice,
            GlobalShaderResources globalShaderResources,
            Sampler aniso4xClampSampler)
            : base(
                graphicsDevice,
                "Terrain",
                new GlobalResourceSetIndices(0u, LightingType.Terrain, 1u, 2u, 3u, null),
                TerrainVertex.VertexDescriptor)
        {
            _aniso4xClampSampler = aniso4xClampSampler;

            _materialResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("TerrainMaterialConstants", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("TileData", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("CliffDetails", ResourceKind.StructuredBufferReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("TextureDetails", ResourceKind.StructuredBufferReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Textures", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("MacroTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("CausticsTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment))));

            _radiusCursorDecalsResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("RadiusCursorDecalTextures", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RadiusCursorDecalSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RadiusCursorDecalConstants", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RadiusCursorDecals", ResourceKind.StructuredBufferReadOnly, ShaderStages.Fragment))));

            var resourceLayouts = new[]
            {
                globalShaderResources.GlobalConstantsResourceLayout,
                globalShaderResources.GlobalLightingConstantsResourceLayout,
                globalShaderResources.GlobalCloudResourceLayout,
                globalShaderResources.GlobalShadowResourceLayout,
                _materialResourceLayout,
                _radiusCursorDecalsResourceLayout,
            };

            Pipeline = AddDisposable(graphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    BlendStateDescription.SingleDisabled,
                    DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                    PrimitiveTopology.TriangleList, // TODO: Use triangle strip
                    ShaderSet.Description,
                    resourceLayouts,
                    RenderPipeline.GameOutputDescription)));
        }

        public ResourceSet CreateMaterialResourceSet(
            DeviceBuffer materialConstantsBuffer,
            Texture tileDataTexture,
            DeviceBuffer cliffDetailsBuffer,
            DeviceBuffer textureDetailsBuffer,
            Texture textureArray,
            Texture macroTexture,
            Texture casuticsTexture)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    _materialResourceLayout,
                    materialConstantsBuffer,
                    tileDataTexture,
                    cliffDetailsBuffer,
                    textureDetailsBuffer,
                    textureArray,
                    macroTexture,
                    casuticsTexture,
                    GraphicsDevice.Aniso4xSampler));
        }

        public ResourceSet CreateRadiusCursorDecalsResourceSet(
            Texture radiusCursorDecalTextureArray,
            DeviceBuffer radiusCursorDecalConstants,
            DeviceBuffer radiusCursorDecalsBuffer)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    _radiusCursorDecalsResourceLayout,
                    radiusCursorDecalTextureArray,
                    _aniso4xClampSampler,
                    radiusCursorDecalConstants,
                    radiusCursorDecalsBuffer));
        }

        [StructLayout(LayoutKind.Explicit, Size = 32)]
        public struct TerrainMaterialConstants
        {
            [FieldOffset(0)]
            public Vector2 MapBorderWidth;

            [FieldOffset(8)]
            public Vector2 MapSize;

            [FieldOffset(16)]
            public bool IsMacroTextureStretched;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TextureInfo
        {
            public uint TextureIndex;
            public uint CellSize;
            public Vector2 _Padding;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CliffInfo
        {
            public const int Size = sizeof(float) * 8;

            public Vector2 BottomLeftUV;
            public Vector2 BottomRightUV;
            public Vector2 TopRightUV;
            public Vector2 TopLeftUV;
        }

        public struct RadiusCursorDecalConstants
        {
            public Vector3 _Padding;
            public uint NumRadiusCursorDecals;
        }

        [StructLayout(LayoutKind.Sequential)]
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
}
