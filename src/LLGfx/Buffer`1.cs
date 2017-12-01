using System.Runtime.InteropServices;
using LLGfx.Util;

namespace LLGfx
{
    public sealed class Buffer<T> : Buffer
        where T : struct
    {
        private static readonly uint SizeOfT = (uint) Marshal.SizeOf<T>();

        public static Buffer<T> CreateStatic(
            GraphicsDevice graphicsDevice,
            T data,
            BufferBindFlags flags)
        {
            byte[] initialData = StructInteropUtility.ToBytes(ref data);

            return new Buffer<T>(
                graphicsDevice,
                SizeOfT,
                1,
                flags,
                ResourceUsage.Static,
                initialData)
            {
                DebugName = $"Static Buffer <{typeof(T).Name}, {flags}>"
            };
        }

        public static Buffer<T> CreateStatic(
            GraphicsDevice graphicsDevice,
            T[] data,
            BufferBindFlags flags)
        {
            byte[] initialData = StructInteropUtility.ToBytes(data);

            return new Buffer<T>(
                graphicsDevice,
                SizeOfT,
                (uint) data.Length,
                flags,
                ResourceUsage.Static,
                initialData)
            {
                DebugName = $"Static Array Buffer <{typeof(T).Name}, {flags}>"
            };
        }

        public static Buffer<T> CreateDynamic(
            GraphicsDevice graphicsDevice,
            BufferBindFlags flags)
        {
            return CreateDynamicArray(graphicsDevice, 1, flags);
        }

        public static Buffer<T> CreateDynamicArray(
            GraphicsDevice graphicsDevice,
            int arrayLength,
            BufferBindFlags flags)
        {
            return new Buffer<T>(
                graphicsDevice,
                SizeOfT,
                (uint) arrayLength,
                flags,
                ResourceUsage.Dynamic,
                null)
            {
                DebugName = $"Dynamic Buffer <{typeof(T).Name}, {arrayLength}, {flags}>"
            };
        }

        private Buffer(
            GraphicsDevice graphicsDevice,
            uint elementSizeInBytes,
            uint elementCount,
            BufferBindFlags flags,
            ResourceUsage usage,
            byte[] initialData)
            : base(graphicsDevice, elementSizeInBytes, elementCount, flags, usage, initialData)
        {

        }

        public void SetData(T[] data)
        {
            EnsureDynamic();

            PlatformSetData(StructInteropUtility.ToBytes(data), data.Length * Marshal.SizeOf<T>());
        }

        public void SetData(ref T data)
        {
            EnsureDynamic();

            PlatformSetData(StructInteropUtility.ToBytes(ref data), Marshal.SizeOf<T>());
        }

        public void SetData(T data)
        {
            EnsureDynamic();

            PlatformSetData(StructInteropUtility.ToBytes(ref data), Marshal.SizeOf<T>());
        }
    }
}
