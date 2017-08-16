namespace LLGfx
{
    public struct PipelineLayoutDescription
    {
        public InlineDescriptorLayoutDescription[] InlineDescriptorLayouts;
        public DescriptorSetLayout[] DescriptorSetLayouts;
        public StaticSamplerDescription[] StaticSamplerStates;
    }

    public struct InlineDescriptorLayoutDescription
    {
        public ShaderStageVisibility Visibility;
        public DescriptorType DescriptorType;
        public int ShaderRegister;
    }
}
