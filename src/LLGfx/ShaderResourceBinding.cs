namespace LLGfx
{
    public sealed class ShaderResourceBinding
    {
        public string Name { get; }
        public ShaderResourceType ResourceType { get; }
        public ShaderType ShaderType { get; }
        public int Slot { get; }

        public ShaderResourceBinding(string name, ShaderResourceType resourceType, ShaderType shaderStage, int slot)
        {
            Name = name;
            ResourceType = resourceType;
            ShaderType = shaderStage;
            Slot = slot;
        }
    }
}
