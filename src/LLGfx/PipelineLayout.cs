namespace LLGfx
{
    public sealed partial class PipelineLayout : GraphicsDeviceChild
    {
        public PipelineLayoutDescription Description { get; }

        public PipelineLayout(GraphicsDevice graphicsDevice, ref PipelineLayoutDescription description)
            : base(graphicsDevice)
        {
            Description = description;

            PlatformConstruct(graphicsDevice, ref description);
        }
    }
}
