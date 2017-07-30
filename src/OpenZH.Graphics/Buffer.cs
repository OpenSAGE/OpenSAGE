namespace OpenZH.Graphics
{
    public abstract partial class Buffer : GraphicsDeviceChild
    {
        public uint SizeInBytes { get; }

        protected Buffer(GraphicsDevice graphicsDevice, uint sizeInBytes)
            : base(graphicsDevice)
        {
            SizeInBytes = PlatformGetAlignedSize(sizeInBytes);
        }
    }
}
