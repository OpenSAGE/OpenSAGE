using System;
using Metal;

namespace OpenZH.Graphics.LowLevel
{
    partial class StaticBuffer
    {
        private void PlatformConstruct<T>(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            T[] data,
            uint dataSizeInBytes)
            where T : struct
        {
            DeviceBuffer = AddDisposable(graphicsDevice.Device.CreateBuffer(
                data,
                MTLResourceOptions.CpuCacheModeDefault));
        }
    }
}