using System;
using System.Runtime.InteropServices;

namespace LLGfx
{
    public sealed partial class DynamicBuffer<T> : Buffer
        where T : struct
    {
        public static DynamicBuffer<T> Create(
            GraphicsDevice graphicsDevice,
            BufferBindFlags flags)
        {
            return CreateArray(graphicsDevice, 1, flags);
        }

        public static DynamicBuffer<T> CreateArray(
            GraphicsDevice graphicsDevice,
            int arrayLength,
            BufferBindFlags flags)
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
            BufferBindFlags flags)
            : base(graphicsDevice, elementSizeInBytes, elementCount, flags)
        {
            PlatformConstruct(
                graphicsDevice,
                SizeInBytes,
                elementSizeInBytes,
                flags);
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
    public enum BufferBindFlags
    {
        None = 0,

        VertexBuffer = 0x1,

        IndexBuffer = 0x2,

        ConstantBuffer = 0x4,

        ShaderResource = 0x8
    }
}
