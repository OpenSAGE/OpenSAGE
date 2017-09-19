namespace LLGfx
{
    public struct PipelineLayoutDescription
    {
        public PipelineLayoutEntry[] Entries;
        public StaticSamplerDescription[] StaticSamplerStates;
    }

    public struct PipelineLayoutEntry
    {
        public PipelineLayoutEntryType EntryType;

        public ShaderStageVisibility Visibility;
        public ResourceType ResourceType;

        // Only relevant if EntryType = Resource
        public ResourcePipelineLayoutItem Resource;

        // Only relevant if EntryType = ResourceView
        public ResourceViewPipelineLayoutItem ResourceView;

        public static PipelineLayoutEntry CreateResource(
            ShaderStageVisibility visibility,
            ResourceType resourceType,
            int shaderRegister)
        {
            return new PipelineLayoutEntry
            {
                EntryType = PipelineLayoutEntryType.Resource,

                Visibility = visibility,
                ResourceType = resourceType,

                Resource = new ResourcePipelineLayoutItem
                {
                    ShaderRegister = shaderRegister
                },

                ResourceView = default(ResourceViewPipelineLayoutItem)
            };
        }

        public static PipelineLayoutEntry CreateResourceView(
            ShaderStageVisibility visibility,
            ResourceType descriptorType,
            int baseShaderRegister,
            int resourceCount)
        {
            return new PipelineLayoutEntry
            {
                EntryType = PipelineLayoutEntryType.ResourceView,

                Visibility = visibility,
                ResourceType = descriptorType,

                Resource = default(ResourcePipelineLayoutItem),

                ResourceView = new ResourceViewPipelineLayoutItem
                {
                    BaseShaderRegister = baseShaderRegister,
                    ResourceCount = resourceCount
                }
            };
        }
    }

    public enum PipelineLayoutEntryType
    {
        Resource,
        ResourceView
    }

    public struct ResourcePipelineLayoutItem
    {
        public int ShaderRegister;
    }

    public struct ResourceViewPipelineLayoutItem
    {
        public int BaseShaderRegister;
        public int ResourceCount;
    }
}
