namespace OpenZH.Graphics
{
    public sealed partial class Buffer : GraphicsDeviceChild
    {
        public Buffer(GraphicsDevice graphicsDevice, uint sizeInBytes)
            : base(graphicsDevice)
        {
            PlatformConstruct(graphicsDevice, sizeInBytes);
        }
    }
}
