using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Data.W3d;
using LLGfx;
using OpenSage.Data;
using OpenSage.Graphics.Util;

namespace OpenSage.Graphics
{
    public sealed class ModelRenderer : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;

        private readonly DescriptorSetLayout _descriptorSetLayoutVertexMaterialPass;
        private readonly DescriptorSetLayout _descriptorSetLayoutPixelMesh;
        private readonly DescriptorSetLayout _descriptorSetLayoutPixelMaterialPass;

        private readonly VertexDescriptor _vertexDescriptor;
        private readonly Shader _vertexShader;
        private readonly Shader _pixelShader;

        private readonly PixelFormat _backBufferFormat;

        private readonly PipelineStateCache _pipelineStateCache;
        private readonly TextureCache _textureCache;

        private readonly PipelineLayout _pipelineLayout;

        private readonly DynamicBuffer _lightingConstantBuffer;
        private LightingConstants _lightingConstants;

        public const int MaxTextures = 32;

        public Texture MissingTexture { get; }

        public ModelRenderer(GraphicsDevice graphicsDevice)
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
                    new DescriptorSetLayoutBinding(DescriptorType.TypedBuffer, 1, 1)
                }
            });

            _pipelineLayout = AddDisposable(new PipelineLayout(graphicsDevice, new PipelineLayoutDescription
            {
                InlineDescriptorLayouts = new[]
                {
                    // PerDrawCB
                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Pixel,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 1
                    },

                    // MeshTransformCB
                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Vertex,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 0
                    },

                    // SkinningCB
                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Vertex,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 1
                    },

                    // LightingCB
                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Pixel,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 0
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

            _vertexShader = AddDisposable(new Shader(shaderLibrary, "MeshVS"));
            _pixelShader = AddDisposable(new Shader(shaderLibrary, "MeshPS"));

            _vertexDescriptor = new VertexDescriptor();
            _vertexDescriptor.SetAttributeDescriptor(0, "POSITION", 0, VertexFormat.Float3, 0, 0);
            _vertexDescriptor.SetAttributeDescriptor(1, "NORMAL", 0, VertexFormat.Float3, 0, 12);
            _vertexDescriptor.SetAttributeDescriptor(2, "BLENDINDICES", 0, VertexFormat.UInt, 0, 24);
            _vertexDescriptor.SetAttributeDescriptor(3, "TEXCOORD", 0, VertexFormat.Float2, 1, 0);
            _vertexDescriptor.SetAttributeDescriptor(4, "TEXCOORD", 1, VertexFormat.Float2, 1, 8);
            _vertexDescriptor.SetLayoutDescriptor(0, 28);
            _vertexDescriptor.SetLayoutDescriptor(1, 16);

            _backBufferFormat = graphicsDevice.BackBufferFormat;

            _pipelineStateCache = AddDisposable(new PipelineStateCache(graphicsDevice));
            _textureCache = AddDisposable(new TextureCache(graphicsDevice));

            _lightingConstantBuffer = AddDisposable(DynamicBuffer.Create<LightingConstants>(graphicsDevice));

            var uploadBatch = new ResourceUploadBatch(_graphicsDevice);
            uploadBatch.Begin();

            MissingTexture = AddDisposable(Texture.CreatePlaceholderTexture2D(
                graphicsDevice,
                uploadBatch));

            uploadBatch.End();
        }

        internal PipelineState GetPipelineState(W3dMesh mesh, W3dShader shader)
        {
            var description = PipelineStateDescription.Default();
            description.PipelineLayout = _pipelineLayout;
            description.RenderTargetFormat = _backBufferFormat;
            description.VertexDescriptor = _vertexDescriptor;
            description.VertexShader = _vertexShader;
            description.PixelShader = _pixelShader;

            description.TwoSided = mesh.Header.Attributes.HasFlag(W3dMeshFlags.TwoSided);

            description.IsDepthWriteEnabled = shader.DepthMask == W3dShaderDepthMask.WriteEnable;

            description.Blending.SourceBlend = shader.SrcBlend.ToBlend();
            description.Blending.DestinationBlend = shader.DestBlend.ToBlend();

            if (shader.SrcBlend != W3dShaderSrcBlendFunc.One || shader.DestBlend != W3dShaderDestBlendFunc.Zero)
            {
                description.Blending.Enabled = true;
            }

            return _pipelineStateCache.GetPipelineState(description);
        }

        internal Texture GetTexture(FileSystemEntry entry, ResourceUploadBatch uploadBatch)
        {
            return _textureCache.GetTexture(entry, uploadBatch);
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
                _descriptorSetLayoutPixelMaterialPass,
                this);

            uploadBatch.End();

            return model;
        }

        public void PreDrawModels(
            CommandEncoder commandEncoder, 
            ref Vector3 cameraPosition)
        {
            commandEncoder.SetPipelineLayout(_pipelineLayout);

            _lightingConstants.CameraPosition = cameraPosition;
            _lightingConstants.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            _lightingConstants.Light0Direction = Vector3.Normalize(new Vector3(-0.3f, -0.8f, -0.2f));
            _lightingConstants.Light0Color = new Vector3(0.7f, 0.7f, 0.8f);
            _lightingConstantBuffer.SetData(ref _lightingConstants);
            commandEncoder.SetInlineConstantBuffer(3, _lightingConstantBuffer);
        }
    }
}
