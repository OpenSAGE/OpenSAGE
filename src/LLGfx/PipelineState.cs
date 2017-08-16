namespace LLGfx
{
    public sealed partial class PipelineState : GraphicsDeviceChild
    {
        public PipelineStateDescription Description { get; }

        public PipelineState(GraphicsDevice graphicsDevice, PipelineStateDescription description)
            : base(graphicsDevice)
        {
            Description = description;

            PlatformConstruct(graphicsDevice, description);
        }
    }
}
