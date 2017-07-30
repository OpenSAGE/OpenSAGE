using System.Runtime.InteropServices;
using Metal;

namespace OpenZH.Graphics
{
    partial class DynamicBuffer
    {
        private void PlatformConstruct(GraphicsDevice graphicsDevice, uint sizeInBytes)
        {
            DeviceBuffer = AddDisposable(graphicsDevice.Device.CreateBuffer(
                sizeInBytes,
                MTLResourceOptions.CpuCacheModeDefault));
        }

        private void PlatformSetData<T>(ref T data)
            where T : struct
        {
            var pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var dataPointer = pinnedArray.AddrOfPinnedObject();

                var dataSize = Marshal.SizeOf<T>();

                unsafe
                {
                    System.Buffer.MemoryCopy(
                        dataPointer.ToPointer(),
                        (DeviceBuffer.Contents).ToPointer(),
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