using System;
using System.Runtime.InteropServices;

namespace LLGfx
{
    public sealed partial class DynamicBuffer<T> : Buffer
        where T : struct
    {
        public static DynamicBuffer<T> Create(
            GraphicsDevice graphicsDevice,
            BufferUsageFlags flags)
        {
            return CreateArray(graphicsDevice, 1, flags);
        }

        public static DynamicBuffer<T> CreateArray(
            GraphicsDevice graphicsDevice,
            int arrayLength,
            BufferUsageFlags flags)
        {
            var elementSizeInBytes = Marshal.SizeOf<T>();

            var result = new DynamicBuffer<T>(
                graphicsDevice,
                (uint) elementSizeInBytes,
                (uint) arrayLength, 
                flags);

            return result;
        }

        private DynamicBuffer(
            GraphicsDevice graphicsDevice,
            uint elementSizeInBytes,
            uint elementCount,
            BufferUsageFlags flags)
            : base(graphicsDevice, elementSizeInBytes, elementCount, flags)
        {
        }

        public void UpdateData(T[] data)
        {
            PlatformSetData(data);
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

    [Flags]
    public enum BufferUsageFlags
    {
        None = 0,

        ConstantBuffer = 0x1,
    }
}
