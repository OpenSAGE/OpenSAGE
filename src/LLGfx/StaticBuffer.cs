using System.Runtime.InteropServices;

namespace LLGfx
{
    public sealed partial class StaticBuffer : Buffer
    {
        public static StaticBuffer Create<T>(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            T[] data,
            bool isConstantBuffer)
            where T : struct
        {
            var elementSizeInBytes = Marshal.SizeOf<T>();
            var sizeInBytes = (uint) (data.Length * elementSizeInBytes);

            var result = new StaticBuffer(
                graphicsDevice,
                sizeInBytes,
                (uint) elementSizeInBytes,
                (uint) data.Length,
                isConstantBuffer);

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
            T data,
            bool isConstantBuffer)
            where T : struct
        {
            return Create(graphicsDevice, uploadBatch, new[] { data }, isConstantBuffer);
        }

        public uint ElementSizeInBytes { get; }
        public uint ElementCount { get; }

        private StaticBuffer(
            GraphicsDevice graphicsDevice,
            uint sizeInBytes,
            uint elementSizeInBytes,
            uint elementCount,
            bool isConstantBuffer)
            : base(graphicsDevice, sizeInBytes, isConstantBuffer)
        {
            ElementSizeInBytes = elementSizeInBytes;
            ElementCount = elementCount;
        }
    }
}
