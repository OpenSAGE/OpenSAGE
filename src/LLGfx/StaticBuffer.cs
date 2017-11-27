using System.Runtime.InteropServices;

namespace LLGfx
{
    public static class StaticBuffer
    {
        public static StaticBuffer<T> Create<T>(
            GraphicsDevice graphicsDevice,
            T[] data,
            BufferBindFlags flags)
            where T : struct
        {
            var elementSizeInBytes = Marshal.SizeOf<T>();

            return new StaticBuffer<T>(
                graphicsDevice,
                (uint) elementSizeInBytes,
                (uint) data.Length,
                data,
                flags);
        }
    }

    public sealed partial class StaticBuffer<T> : Buffer
        where T : struct
    {
        internal StaticBuffer(
            GraphicsDevice graphicsDevice,
            uint elementSizeInBytes,
            uint elementCount,
            T[] data,
            BufferBindFlags flags)
            : base(graphicsDevice, elementSizeInBytes, elementCount, flags)
        {
            PlatformConstruct(
                graphicsDevice,
                data,
                SizeInBytes,
                elementSizeInBytes,
                flags);
        }
    }
}
