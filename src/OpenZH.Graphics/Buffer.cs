namespace OpenZH.Graphics
{
    public sealed partial class Buffer : GraphicsDeviceChild
    {
        public uint SizeInBytes { get; }

        public Buffer(GraphicsDevice graphicsDevice, uint sizeInBytes)
            : base(graphicsDevice)
        {
            var alignedSizeInBytes = PlatformGetAlignedSize(sizeInBytes);

            SizeInBytes = alignedSizeInBytes;

            PlatformConstruct(graphicsDevice, alignedSizeInBytes);
        }

        public void SetData<T>(T data, int offset)
            where T : struct
        {
            PlatformSetData(data, offset);
        }

        public void SetData<T>(T[] data, int offset)
            where T : struct
        {
            PlatformSetData(data, offset);
        }
    }
}
