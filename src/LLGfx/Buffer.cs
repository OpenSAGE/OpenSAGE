namespace LLGfx
{
    public abstract partial class Buffer : GraphicsDeviceChild
    {
        public uint SizeInBytes { get; }

        protected Buffer(GraphicsDevice graphicsDevice, uint sizeInBytes, bool isConstantBuffer)
            : base(graphicsDevice)
        {
            SizeInBytes = isConstantBuffer
                ? PlatformGetAlignedSize(sizeInBytes)
                : sizeInBytes;
        }
    }
}
