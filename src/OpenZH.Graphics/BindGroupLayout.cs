namespace OpenZH.Graphics
{
    public sealed partial class BindGroupLayout
    {
        public BindGroupLayoutDescription Description { get; }

        public BindGroupLayout(BindGroupLayoutDescription description)
        {
            Description = description;

            PlatformConstruct(description);
        }
    }
}
