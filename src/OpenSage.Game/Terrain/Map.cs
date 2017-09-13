using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.Graphics.Util;

namespace OpenSage.Terrain
{
    public sealed class Map : GraphicsObject
    {
        public const int MaxTextures = 48;

        private readonly Terrain _terrain;

        private readonly PipelineLayout _pipelineLayout;
        private readonly PipelineState _pipelineState;

        private readonly DynamicBuffer _transformConstantBuffer;
        private TransformConstants _transformConstants;

        private readonly DynamicBuffer _lightingConstantBuffer;
        private LightingConstants _lightingConstants;

        public Map(
            MapFile mapFile, 
            FileSystem fileSystem,
            GraphicsDevice graphicsDevice)
        {
            var descriptorSetLayoutPixelTerrain = new DescriptorSetLayout(new DescriptorSetLayoutDescription
            {
                Visibility = ShaderStageVisibility.Pixel,
                Bindings = new[]
                {
                    // TextureInfo[]
                    new DescriptorSetLayoutBinding(DescriptorType.StructuredBuffer, 1, 1),
                    
                    // Textures[]
                    new DescriptorSetLayoutBinding(DescriptorType.Texture, 2, MaxTextures),
                }
            });

            var descriptorSetLayoutPixelTerrainPatch = new DescriptorSetLayout(new DescriptorSetLayoutDescription
            {
                Visibility = ShaderStageVisibility.Pixel,
                Bindings = new[]
                {
                    // TextureIndices
                    new DescriptorSetLayoutBinding(DescriptorType.TypedBuffer, 0, 1)
                }
            });

            _pipelineLayout = AddDisposable(new PipelineLayout(graphicsDevice, new PipelineLayoutDescription
            {
                InlineDescriptorLayouts = new [] 
                {
                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Vertex,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 0
                    },

                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Pixel,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 0
                    },
                },
                DescriptorSetLayouts = new[]
                {
                    descriptorSetLayoutPixelTerrainPatch,
                    descriptorSetLayoutPixelTerrain
                },
                StaticSamplerStates = new[]
                {
                    new StaticSamplerDescription
                    {
                        Visibility = ShaderStageVisibility.Pixel,
                        ShaderRegister = 0,
                        SamplerStateDescription = new SamplerStateDescription
                        {
                            Filter = SamplerFilter.Anisotropic
                        }
                    }
                }
            }));

            var shaderLibrary = AddDisposable(new ShaderLibrary(graphicsDevice));
            var vertexShader = AddDisposable(new Shader(shaderLibrary, "TerrainVS"));
            var pixelShader = AddDisposable(new Shader(shaderLibrary, "TerrainPS"));

            var vertexDescriptor = new VertexDescriptor();
            vertexDescriptor.SetAttributeDescriptor(0, "POSITION", 0, VertexFormat.Float3, 0, 0);
            vertexDescriptor.SetAttributeDescriptor(1, "NORMAL", 0, VertexFormat.Float3, 0, 12);
            vertexDescriptor.SetAttributeDescriptor(2, "TEXCOORD", 0, VertexFormat.Float2, 0, 24);
            vertexDescriptor.SetLayoutDescriptor(0, 32);

            var pipelineStateDescription = PipelineStateDescription.Default();
            pipelineStateDescription.PipelineLayout = _pipelineLayout;
            pipelineStateDescription.RenderTargetFormat = graphicsDevice.BackBufferFormat;
            pipelineStateDescription.VertexDescriptor = vertexDescriptor;
            pipelineStateDescription.VertexShader = vertexShader;
            pipelineStateDescription.PixelShader = pixelShader;

            _pipelineState = AddDisposable(new PipelineState(graphicsDevice, pipelineStateDescription));

            _transformConstantBuffer = AddDisposable(DynamicBuffer.Create<TransformConstants>(graphicsDevice));

            _lightingConstantBuffer = AddDisposable(DynamicBuffer.Create<LightingConstants>(graphicsDevice));

            var textureCache = AddDisposable(new TextureCache(graphicsDevice));

            _terrain = AddDisposable(new Terrain(
                mapFile, 
                graphicsDevice,
                fileSystem,
                textureCache,
                descriptorSetLayoutPixelTerrain,
                descriptorSetLayoutPixelTerrainPatch));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TransformConstants
        {
            public Matrix4x4 WorldViewProjection;
            public Matrix4x4 World;
        }

        [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        private struct LightingConstants
        {
            public const int SizeInBytes = 64;

            [FieldOffset(0)]
            public Vector3 CameraPosition;

            [FieldOffset(16)]
            public Vector3 AmbientLightColor;

            [FieldOffset(32)]
            public Vector3 Light0Direction;

            [FieldOffset(48)]
            public Vector3 Light0Color;
        }

        public void Draw(CommandEncoder commandEncoder,
            ref Vector3 cameraPosition,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            commandEncoder.SetPipelineLayout(_pipelineLayout);

            commandEncoder.SetPipelineState(_pipelineState);

            _transformConstants.WorldViewProjection = view * projection;
            _transformConstants.World = Matrix4x4.Identity;
            _transformConstantBuffer.SetData(ref _transformConstants);
            commandEncoder.SetInlineConstantBuffer(0, _transformConstantBuffer);

            _lightingConstants.CameraPosition = cameraPosition;
            _lightingConstants.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            _lightingConstants.Light0Direction = Vector3.Normalize(new Vector3(-0.3f, -0.8f, -0.2f));
            _lightingConstants.Light0Color = new Vector3(0.7f, 0.7f, 0.8f);
            _lightingConstantBuffer.SetData(ref _lightingConstants);
            commandEncoder.SetInlineConstantBuffer(1, _lightingConstantBuffer);

            _terrain.Draw(commandEncoder);           
        }
    }
}
