namespace OpenZH.Graphics
{
    public sealed partial class DescriptorSetLayout
    {
        public DescriptorSetLayoutDescription Description { get; }

        public DescriptorSetLayout(DescriptorSetLayoutDescription description)
        {
            Description = description;

            PlatformConstruct(description);
        }
    }
}
