using System.Runtime.InteropServices;
using Foundation;
using Metal;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class Buffer
    {
        private const int ConstantBufferAlignment = 256;

        private const int ConstantBufferAlignmentMask = ConstantBufferAlignment - 1;

        internal IMTLBuffer DeviceBuffer { get; set; }

        internal override string PlatformGetDebugName() => DeviceBuffer.Label;
        internal override void PlatformSetDebugName(string value) => DeviceBuffer.Label = value;

        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            uint sizeInBytes,
            uint elementSizeInBytes,
            BufferBindFlags flags,
            ResourceUsage usage,
            byte[] initialData)
        {
            if (usage == ResourceUsage.Static)
            {
                DeviceBuffer = AddDisposable(graphicsDevice.Device.CreateBuffer(
                    initialData,
                    MTLResourceOptions.StorageModeManaged));
            }
            else
            {
                DeviceBuffer = AddDisposable(graphicsDevice.Device.CreateBuffer(
                    sizeInBytes,
                    MTLResourceOptions.CpuCacheModeWriteCombined | MTLResourceOptions.StorageModeManaged));
            }
        }

        private uint PlatformGetAlignedSize(uint sizeInBytes)
        {
            // Align to 256 bytes.
            return (uint) ((sizeInBytes + ConstantBufferAlignmentMask) & ~ConstantBufferAlignmentMask);
        }

        internal void PlatformSetData(byte[] data, int dataSizeInBytes)
        {
            Marshal.Copy(data, 0, DeviceBuffer.Contents, dataSizeInBytes);

            DeviceBuffer.DidModify(new NSRange(0, dataSizeInBytes));
        }
    }
}
