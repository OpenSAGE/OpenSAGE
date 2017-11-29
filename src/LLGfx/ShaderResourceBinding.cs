namespace LLGfx
{
    public sealed class ShaderResourceBinding
    {
        public string Name { get; }
        public ShaderResourceType ResourceType { get; }
        public ShaderType ShaderType { get; }
        public int Slot { get; }

        public int ConstantBufferSizeInBytes { get; }
        public ConstantBufferField[] ConstantBufferFields { get; }

        public ShaderResourceBinding(
            string name, 
            ShaderResourceType resourceType, 
            ShaderType shaderStage, 
            int slot,
            int constantBufferSizeInBytes,
            ConstantBufferField[] constantBufferFields)
        {
            Name = name;
            ResourceType = resourceType;
            ShaderType = shaderStage;
            Slot = slot;

            ConstantBufferSizeInBytes = constantBufferSizeInBytes;
            ConstantBufferFields = constantBufferFields;
        }
    }
}
