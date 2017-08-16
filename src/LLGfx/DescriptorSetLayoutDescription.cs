namespace LLGfx
{
    public struct DescriptorSetLayoutDescription
    {
        public ShaderStageVisibility Visibility;

        public DescriptorSetLayoutBinding[] Bindings;
    }

    public struct DescriptorSetLayoutBinding
    {
        public DescriptorType DescriptorType;
        public int BaseShaderRegister;
        public int DescriptorCount;

        public DescriptorSetLayoutBinding(DescriptorType descriptorType, int baseShaderRegister, int descriptorCount)
        {
            DescriptorType = descriptorType;
            BaseShaderRegister = baseShaderRegister;
            DescriptorCount = descriptorCount;
        }
    }
}
