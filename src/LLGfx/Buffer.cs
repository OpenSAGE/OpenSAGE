namespace LLGfx
{
    public abstract partial class Buffer : GraphicsDeviceChild
    {
        public uint ElementSizeInBytes { get; }
        public uint ElementCount { get; }

        public uint SizeInBytes { get; }

        public BufferUsageFlags UsageFlags { get; }

        protected Buffer(
            GraphicsDevice graphicsDevice, 
            uint elementSizeInBytes, 
            uint elementCount,
            BufferUsageFlags flags)
            : base(graphicsDevice)
        {
            ElementSizeInBytes = elementSizeInBytes;
            ElementCount = elementCount;

            var alignedElementSize = flags.HasFlag(BufferUsageFlags.ConstantBuffer)
                ? PlatformGetAlignedSize(elementSizeInBytes)
                : elementSizeInBytes;

            SizeInBytes = alignedElementSize * elementCount;

            UsageFlags = flags;
        }
    }
}
