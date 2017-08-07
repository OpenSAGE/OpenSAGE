namespace OpenZH.Graphics.LowLevel
{
    public sealed partial class PipelineLayout : GraphicsDeviceChild
    {
        public PipelineLayoutDescription Description {get;}
        public PipelineLayout(GraphicsDevice graphicsDevice, PipelineLayoutDescription description)
            : base(graphicsDevice)
        {
            Description = description;

            PlatformConstruct(graphicsDevice, description);
        }
    }
}
