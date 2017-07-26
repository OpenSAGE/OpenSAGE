using System.Runtime.InteropServices;
using Metal;

namespace OpenZH.Graphics
{
    partial class Buffer
    {
        internal IMTLBuffer DeviceBuffer { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, uint sizeInBytes)
        {
            DeviceBuffer = AddDisposable(graphicsDevice.Device.CreateBuffer(
                sizeInBytes,
                MTLResourceOptions.CpuCacheModeDefault));
        }

        private void PlatformSetData<T>(T data, int offset)
            where T : struct
        {
            PlatformSetData(new T[] { data }, offset);
        }

        private void PlatformSetData<T>(T[] data, int offset)
            where T : struct
        {
            var pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var dataPointer = pinnedArray.AddrOfPinnedObject();

                var dataSize = Marshal.SizeOf<T>() * data.Length;

                unsafe
                {
                    System.Buffer.MemoryCopy(
                        dataPointer.ToPointer(),
                        (DeviceBuffer.Contents + offset).ToPointer(),
                        dataSize,
                        dataSize);
                }
            }
            finally
            {
                pinnedArray.Free();
            }
        }
    }
}