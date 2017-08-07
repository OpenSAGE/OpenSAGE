using System.Runtime.InteropServices;

namespace OpenZH.Graphics.LowLevel
{
    public sealed partial class StaticBuffer : Buffer
    {
        public static StaticBuffer Create<T>(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            T[] data)
            where T : struct
        {
            var elementSizeInBytes = Marshal.SizeOf<T>();
            var sizeInBytes = (uint) (data.Length * elementSizeInBytes);

            var result = new StaticBuffer(
                graphicsDevice,
                sizeInBytes,
                (uint) elementSizeInBytes,
                (uint) data.Length);

            result.PlatformConstruct(
                graphicsDevice,
                uploadBatch,
                data,
                sizeInBytes);

            return result;
        }

        public static StaticBuffer Create<T>(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            T data)
            where T : struct
        {
            return Create(graphicsDevice, uploadBatch, new[] { data });
        }

        public uint ElementSizeInBytes { get; }
        public uint ElementCount { get; }

        private StaticBuffer(
            GraphicsDevice graphicsDevice,
            uint sizeInBytes,
            uint elementSizeInBytes,
            uint elementCount)
            : base(graphicsDevice, sizeInBytes)
        {
            ElementSizeInBytes = elementSizeInBytes;
            ElementCount = elementCount;
        }
    }
}
