using LLGfx;

namespace OpenSage.Graphics.Effects
{
    public sealed class TerrainEffect : Effect
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly DescriptorSetLayout _terrainDescriptorSetLayout;

        public TerrainEffect(GraphicsDevice graphicsDevice, int numTextures)
            : base(graphicsDevice, "TerrainVS", "TerrainPS")
        {
            _graphicsDevice = graphicsDevice;

            _terrainDescriptorSetLayout = new DescriptorSetLayout(new DescriptorSetLayoutDescription
            {
                Visibility = ShaderStageVisibility.Pixel,
                Bindings = new[]
                {
                    // TileData
                    new DescriptorSetLayoutBinding(DescriptorType.Texture, 0, 1),

                    // CliffDetails
                    new DescriptorSetLayoutBinding(DescriptorType.StructuredBuffer, 1, 1),

                    // TextureDetails
                    new DescriptorSetLayoutBinding(DescriptorType.StructuredBuffer, 2, 1),
                    
                    // Textures[]
                    new DescriptorSetLayoutBinding(DescriptorType.Texture, 3, numTextures),
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
            vertexDescriptor.SetAttributeDescriptor(2, "TEXCOORD", 0, VertexFormat.Float2, 0, 24);
            vertexDescriptor.SetLayoutDescriptor(0, 32);
        }

        private void CreatePipelineLayoutDescription(out PipelineLayoutDescription pipelineLayoutDescription)
        {
            pipelineLayoutDescription = new PipelineLayoutDescription
            {
                InlineDescriptorLayouts = new[]
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
                    _terrainDescriptorSetLayout
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

        public PipelineState GetPipelineState(bool wireframe)
        {
            var description = PipelineStateDescription.Default();

            if (wireframe)
            {
                description.FillMode = FillMode.Wireframe;
                description.IsDepthWriteEnabled = false;
            }

            return GetPipelineState(ref description);
        }

        public DescriptorSet CreateTerrainDescriptorSet()
        {
            return new DescriptorSet(
                _graphicsDevice,
                _terrainDescriptorSetLayout);
        }
    }
}
