using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Data.W3d;
using LLGfx;
using System.Collections.Generic;
using OpenSage.Data;

namespace OpenSage.Graphics
{
    public sealed class ModelRenderer : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;

        private readonly DescriptorSetLayout _descriptorSetLayoutVertexMaterialPass;
        private readonly DescriptorSetLayout _descriptorSetLayoutPixelMesh;
        private readonly DescriptorSetLayout _descriptorSetLayoutPixelMaterialPass;

        private readonly PipelineLayout _pipelineLayout;
        private readonly PipelineState _pipelineState;

        private readonly DynamicBuffer _lightingConstantBuffer;
        private LightingConstants _lightingConstants;

        public const int MaxTextures = 32;

        public ModelRenderer(
            GraphicsDevice graphicsDevice, 
            SwapChain swapChain)
        {
            _graphicsDevice = graphicsDevice;

            _descriptorSetLayoutVertexMaterialPass = new DescriptorSetLayout(new DescriptorSetLayoutDescription
            {
                Visibility = ShaderStageVisibility.Vertex,
                Bindings = new[]
                {
                    // MaterialIndices[]
                    new DescriptorSetLayoutBinding(DescriptorType.TypedBuffer, 0, 1)
                }
            });

            _descriptorSetLayoutPixelMesh = new DescriptorSetLayout(new DescriptorSetLayoutDescription
            {
                Visibility = ShaderStageVisibility.Pixel,
                Bindings = new[]
                {
                    // Materials
                    new DescriptorSetLayoutBinding(DescriptorType.StructuredBuffer, 0, 1),

                    // Textures[]
                    new DescriptorSetLayoutBinding(DescriptorType.Texture, 2, MaxTextures)
                }
            });

            _descriptorSetLayoutPixelMaterialPass = new DescriptorSetLayout(new DescriptorSetLayoutDescription
            {
                Visibility = ShaderStageVisibility.Pixel,
                Bindings = new[]
                {
                    // TextureIndices
                    new DescriptorSetLayoutBinding(DescriptorType.TypedBuffer, 1, 1),
                }
            });

            _pipelineLayout = AddDisposable(new PipelineLayout(graphicsDevice, new PipelineLayoutDescription
            {
                InlineDescriptorLayouts = new[]
                {
                    // MeshTransformCB
                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Vertex,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 0
                    },

                    // LightingCB
                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Pixel,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 0
                    },

                    // PerDrawCB
                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Pixel,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 1
                    },
                },
                DescriptorSetLayouts = new[]
                {
                    // Sorted in descending frequency of updating
                    _descriptorSetLayoutVertexMaterialPass,
                    _descriptorSetLayoutPixelMaterialPass,
                    _descriptorSetLayoutPixelMesh
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

            var pixelShader = AddDisposable(new Shader(shaderLibrary, "MeshPS"));
            var vertexShader = AddDisposable(new Shader(shaderLibrary, "MeshVS"));

            var vertexDescriptor = new VertexDescriptor();
            vertexDescriptor.SetAttributeDescriptor(0, "POSITION", 0, VertexFormat.Float3, 0, 0);
            vertexDescriptor.SetAttributeDescriptor(1, "NORMAL", 0, VertexFormat.Float3, 0, 12);
            vertexDescriptor.SetAttributeDescriptor(2, "TEXCOORD", 0, VertexFormat.Float2, 1, 0);
            vertexDescriptor.SetLayoutDescriptor(0, 24);
            vertexDescriptor.SetLayoutDescriptor(1, 8);

            _pipelineState = AddDisposable(new PipelineState(graphicsDevice, new PipelineStateDescription
            {
                PipelineLayout = _pipelineLayout,
                PixelShader = pixelShader,
                RenderTargetFormat = swapChain.BackBufferFormat,
                VertexDescriptor = vertexDescriptor,
                VertexShader = vertexShader
            }));

            _lightingConstantBuffer = AddDisposable(DynamicBuffer.Create<LightingConstants>(graphicsDevice));
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

        public Model LoadModel(W3dFile w3dFile, FileSystem fileSystem, GraphicsDevice graphicsDevice)
        {
            var uploadBatch = new ResourceUploadBatch(_graphicsDevice);
            uploadBatch.Begin();

            var model = new Model(
                graphicsDevice,
                uploadBatch,
                w3dFile,
                fileSystem,
                _descriptorSetLayoutPixelMesh,
                _descriptorSetLayoutVertexMaterialPass,
                _descriptorSetLayoutPixelMaterialPass);

            uploadBatch.End();

            return model;
        }

        public void PreDrawModels(
            CommandEncoder commandEncoder, 
            ref Vector3 cameraPosition)
        {
            commandEncoder.SetPipelineState(_pipelineState);

            commandEncoder.SetPipelineLayout(_pipelineLayout);

            _lightingConstants.CameraPosition = cameraPosition;
            _lightingConstants.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            _lightingConstants.Light0Direction = Vector3.Normalize(new Vector3(-0.3f, -0.8f, -0.2f));
            _lightingConstants.Light0Color = new Vector3(0.5f, 0.5f, 0.5f);
            _lightingConstantBuffer.SetData(ref _lightingConstants);
            commandEncoder.SetInlineConstantBuffer(1, _lightingConstantBuffer);
        }
    }
}
