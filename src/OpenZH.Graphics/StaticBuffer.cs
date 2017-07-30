using System.Runtime.InteropServices;

namespace OpenZH.Graphics
{
    public sealed partial class StaticBuffer : Buffer
    {
        public static StaticBuffer Create<T>(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            T[] data)
            where T : struct
        {
            var sizeInBytes = (uint) (data.Length * Marshal.SizeOf<T>());

            var result = new StaticBuffer(graphicsDevice, sizeInBytes);

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

        private StaticBuffer(GraphicsDevice graphicsDevice, uint sizeInBytes)
            : base(graphicsDevice, sizeInBytes)
        {
        }
    }
}
