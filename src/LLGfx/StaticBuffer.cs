using System.Runtime.InteropServices;

namespace LLGfx
{
    public static class StaticBuffer
    {
        public static StaticBuffer<T> Create<T>(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            T[] data)
            where T : struct
        {
            var elementSizeInBytes = Marshal.SizeOf<T>();
            var sizeInBytes = (uint) (data.Length * elementSizeInBytes);

            return new StaticBuffer<T>(
                graphicsDevice,
                sizeInBytes,
                (uint) elementSizeInBytes,
                (uint) data.Length,
                uploadBatch,
                data);
        }
    }

    public sealed partial class StaticBuffer<T> : Buffer
        where T : struct
    {
        public uint ElementSizeInBytes { get; }
        public uint ElementCount { get; }

        internal StaticBuffer(
            GraphicsDevice graphicsDevice,
            uint sizeInBytes,
            uint elementSizeInBytes,
            uint elementCount,
            ResourceUploadBatch uploadBatch,
            T[] data)
            : base(graphicsDevice, sizeInBytes, false)
        {
            ElementSizeInBytes = elementSizeInBytes;
            ElementCount = elementCount;

            PlatformConstruct(
                graphicsDevice,
                uploadBatch,
                data,
                sizeInBytes);
        }
    }
}
