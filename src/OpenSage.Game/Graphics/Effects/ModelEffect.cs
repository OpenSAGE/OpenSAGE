using LLGfx;
using OpenSage.Data.W3d;
using OpenSage.Graphics.Util;

namespace OpenSage.Graphics.Effects
{
    public sealed class ModelEffect : Effect
    {
        public const int MaxTextures = 32;

        private readonly DescriptorSetLayout _descriptorSetLayoutVertexMaterialPass;
        private readonly DescriptorSetLayout _descriptorSetLayoutPixelMesh;
        private readonly DescriptorSetLayout _descriptorSetLayoutPixelMaterialPass;

        public ModelEffect(GraphicsDevice graphicsDevice)
            : base(graphicsDevice, "MeshVS", "MeshPS")
        {
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

            CreateVertexDescriptor(out var vertexDescriptor);
            CreatePipelineLayoutDescription(out var pipelineLayoutDescription);
            Initialize(ref vertexDescriptor, ref pipelineLayoutDescription);
        }

        private void CreateVertexDescriptor(out VertexDescriptor vertexDescriptor)
        {
            vertexDescriptor = new VertexDescriptor();
            vertexDescriptor.SetAttributeDescriptor(0, "POSITION", 0, VertexFormat.Float3, 0, 0);
            vertexDescriptor.SetAttributeDescriptor(1, "NORMAL", 0, VertexFormat.Float3, 0, 12);
            vertexDescriptor.SetAttributeDescriptor(2, "BLENDINDICES", 0, VertexFormat.UInt, 0, 24);
            vertexDescriptor.SetAttributeDescriptor(3, "TEXCOORD", 0, VertexFormat.Float2, 1, 0);
            vertexDescriptor.SetAttributeDescriptor(4, "TEXCOORD", 1, VertexFormat.Float2, 1, 8);
            vertexDescriptor.SetLayoutDescriptor(0, 28);
            vertexDescriptor.SetLayoutDescriptor(1, 16);
        }

        private void CreatePipelineLayoutDescription(out PipelineLayoutDescription pipelineLayoutDescription)
        {
            pipelineLayoutDescription = new PipelineLayoutDescription
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
            };
        }

        public PipelineState GetPipelineState(W3dMesh mesh, W3dShader shader)
        {
            var description = PipelineStateDescription.Default();

            description.TwoSided = mesh.Header.Attributes.HasFlag(W3dMeshFlags.TwoSided);

            description.IsDepthWriteEnabled = shader.DepthMask == W3dShaderDepthMask.WriteEnable;

            if (shader.SrcBlend != W3dShaderSrcBlendFunc.One || shader.DestBlend != W3dShaderDestBlendFunc.Zero)
            {
                description.Blending.Enabled = true;
            }

            description.Blending.SourceBlend = shader.SrcBlend.ToBlend();
            description.Blending.DestinationBlend = shader.DestBlend.ToBlend();

            return GetPipelineState(ref description);
        }

        public DescriptorSet CreateMeshPixelDescriptorSet()
        {
            return new DescriptorSet(
                GraphicsDevice,
                _descriptorSetLayoutPixelMesh);
        }

        public DescriptorSet CreateMaterialPassVertexDescriptorSet()
        {
            return new DescriptorSet(
                GraphicsDevice,
                _descriptorSetLayoutVertexMaterialPass);
        }

        public DescriptorSet CreateMaterialPassPixelDescriptorSet()
        {
            return new DescriptorSet(
                GraphicsDevice,
                _descriptorSetLayoutPixelMaterialPass);
        }
    }
}
