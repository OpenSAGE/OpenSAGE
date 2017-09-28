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

            return new StaticBuffer<T>(
                graphicsDevice,
                (uint) elementSizeInBytes,
                (uint) data.Length,
                uploadBatch,
                data);
        }
    }

    public sealed partial class StaticBuffer<T> : Buffer
        where T : struct
    {
        private ShaderResourceView _shaderResourceView;

        internal ShaderResourceView ShaderResourceView
        {
            get
            {
                if (_shaderResourceView == null)
                {
                    _shaderResourceView = AddDisposable(ShaderResourceView.Create(GraphicsDevice, this));
                }
                return _shaderResourceView;
            }
        }

        internal StaticBuffer(
            GraphicsDevice graphicsDevice,
            uint elementSizeInBytes,
            uint elementCount,
            ResourceUploadBatch uploadBatch,
            T[] data)
            : base(graphicsDevice, elementSizeInBytes, elementCount, BufferUsageFlags.None)
        {
            PlatformConstruct(
                graphicsDevice,
                uploadBatch,
                data,
                SizeInBytes);
        }
    }
}
