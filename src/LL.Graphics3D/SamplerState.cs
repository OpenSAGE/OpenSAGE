namespace LL.Graphics3D
{
    public sealed partial class SamplerState : GraphicsDeviceChild
    {
        public SamplerStateDescription Description { get; }

        public SamplerState(GraphicsDevice graphicsDevice, SamplerStateDescription description)
            : base(graphicsDevice)
        {
            Description = description;

            PlatformConstruct(graphicsDevice, description);
        }
    }
}
