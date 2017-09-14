using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.Graphics.Util;
using OpenSage.Terrain.Util;

namespace OpenSage.Terrain
{
    public sealed class Map : GraphicsObject
    {
        public const int MaxTextures = 48;

        private readonly Terrain _terrain;

        private readonly PipelineLayout _pipelineLayout;
        private readonly PipelineState _pipelineState;
        private readonly PipelineState _pipelineStateWireframe;

        private readonly DynamicBuffer _transformConstantBuffer;
        private TransformConstants _transformConstants;

        private readonly Dictionary<TimeOfDay, LightConfiguration> _lightConfigurations;

        private readonly DynamicBuffer _lightingConstantBuffer;
        private LightingConstants _lightingConstants;

        public Terrain Terrain => _terrain;

        private TimeOfDay _currentTimeOfDay;
        public TimeOfDay CurrentTimeOfDay
        {
            get { return _currentTimeOfDay; }
            set
            {
                _currentTimeOfDay = value;

                var lightConfiguration = _lightConfigurations[value];
                _lightingConstants.Light0 = lightConfiguration.Light0;
                _lightingConstants.Light1 = lightConfiguration.Light1;
                _lightingConstants.Light2 = lightConfiguration.Light2;
            }
        }

        public bool RenderWireframeOverlay { get; set; }

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
                    // TileData
                    new DescriptorSetLayoutBinding(DescriptorType.Texture, 0, 1),

                    // TextureDetails
                    new DescriptorSetLayoutBinding(DescriptorType.StructuredBuffer, 1, 1),
                    
                    // Textures[]
                    new DescriptorSetLayoutBinding(DescriptorType.Texture, 2, MaxTextures),
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

            pipelineStateDescription.FillMode = FillMode.Wireframe;
            pipelineStateDescription.IsDepthWriteEnabled = false;
            _pipelineStateWireframe = AddDisposable(new PipelineState(graphicsDevice, pipelineStateDescription));

            _transformConstantBuffer = AddDisposable(DynamicBuffer.Create<TransformConstants>(graphicsDevice));

            _lightingConstantBuffer = AddDisposable(DynamicBuffer.Create<LightingConstants>(graphicsDevice));

            _lightConfigurations = new Dictionary<TimeOfDay, LightConfiguration>();
            foreach (var kvp in mapFile.GlobalLighting.LightingConfigurations)
            {
                _lightConfigurations[kvp.Key] = ToLightConfiguration(kvp.Value);
            }

            CurrentTimeOfDay = mapFile.GlobalLighting.Time;

            var textureCache = AddDisposable(new TextureCache(graphicsDevice));

            _terrain = AddDisposable(new Terrain(
                mapFile, 
                graphicsDevice,
                fileSystem,
                textureCache,
                descriptorSetLayoutPixelTerrain));
        }

        private sealed class LightConfiguration
        {
            public Light Light0;
            public Light Light1;
            public Light Light2;
        }

        private static LightConfiguration ToLightConfiguration(GlobalLightingConfiguration mapLightingConfiguration)
        {
            return new LightConfiguration
            {
                Light0 = ToLight(mapLightingConfiguration.TerrainSun),
                Light1 = ToLight(mapLightingConfiguration.TerrainAccent1),
                Light2 = ToLight(mapLightingConfiguration.TerrainAccent2),
            };
        }

        private static Light ToLight(GlobalLight mapLight)
        {
            //var directionQuaternion = Quaternion.CreateFromYawPitchRoll(
            //    mapLight.EulerAngles.Z,
            //    mapLight.EulerAngles.X,
            //    mapLight.EulerAngles.Y);

            //var direction = Vector3.Normalize(Vector3.Transform(-Vector3.UnitY, directionQuaternion));

            return new Light
            {
                Ambient = mapLight.Ambient.ToVector3(),
                Color = mapLight.Color.ToVector3(),
                Direction = Vector3.Normalize(new Vector3(
                    mapLight.EulerAngles.X,
                    mapLight.EulerAngles.Y,
                    mapLight.EulerAngles.Z))
            };
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TransformConstants
        {
            public Matrix4x4 WorldViewProjection;
            public Matrix4x4 World;
        }

        [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        private struct Light
        {
            public const int SizeInBytes = 48;

            [FieldOffset(0)]
            public Vector3 Ambient;

            [FieldOffset(16)]
            public Vector3 Color;

            [FieldOffset(32)]
            public Vector3 Direction;
        };

        [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        private struct LightingConstants
        {
            public const int SizeInBytes = 160;

            [FieldOffset(0)]
            public Vector3 CameraPosition;

            [FieldOffset(16)]
            public Light Light0;

            [FieldOffset(64)]
            public Light Light1;

            [FieldOffset(112)]
            public Light Light2;
        }

        public void Draw(CommandEncoder commandEncoder,
            ref Vector3 cameraPosition,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            commandEncoder.SetPipelineLayout(_pipelineLayout);

            Draw(commandEncoder, 
                _pipelineState, 
                ref _lightingConstants, 
                ref cameraPosition, 
                ref view, 
                ref projection);

            if (RenderWireframeOverlay)
            {
                var nullLightingConstants = new LightingConstants();
                Draw(commandEncoder, 
                    _pipelineStateWireframe, 
                    ref nullLightingConstants, 
                    ref cameraPosition, 
                    ref view, 
                    ref projection);
            }
        }

        private void Draw(
            CommandEncoder commandEncoder,
            PipelineState pipelineState,
            ref LightingConstants lightingConstants,
            ref Vector3 cameraPosition,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            commandEncoder.SetPipelineState(pipelineState);

            _transformConstants.WorldViewProjection = view * projection;
            _transformConstants.World = Matrix4x4.Identity;
            _transformConstantBuffer.SetData(ref _transformConstants);
            commandEncoder.SetInlineConstantBuffer(0, _transformConstantBuffer);

            _lightingConstants.CameraPosition = cameraPosition;
            _lightingConstantBuffer.SetData(ref lightingConstants);
            commandEncoder.SetInlineConstantBuffer(1, _lightingConstantBuffer);

            _terrain.Draw(commandEncoder);
        }
    }
}
