using System.Runtime.InteropServices;

namespace LLGfx
{
    public sealed partial class DynamicBuffer<T> : Buffer
        where T : struct
    {
        public static DynamicBuffer<T> Create(
            GraphicsDevice graphicsDevice)
        {
            var sizeInBytes = (uint) Marshal.SizeOf<T>();

            var result = new DynamicBuffer<T>(graphicsDevice, sizeInBytes);

            result.PlatformConstruct(
                graphicsDevice,
                sizeInBytes);

            return result;
        }

        private DynamicBuffer(GraphicsDevice graphicsDevice, uint sizeInBytes)
            : base(graphicsDevice, sizeInBytes, true)
        {
        }

        public void UpdateData(ref T data)
        {
            PlatformSetData(ref data);
        }

        public void UpdateData(T data)
        {
            PlatformSetData(ref data);
        }
    }

    public delegate void DynamicBufferUpdateDataDelegate<T>(ref T data)
        where T : struct;
}
