using SharpDX;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class Buffer
    {
        internal Resource DeviceBuffer { get; /* protected */ set; }

        internal abstract long DeviceCurrentGPUVirtualAddress { get; }

        private uint PlatformGetAlignedSize(uint sizeInBytes)
        {
            // Align to 256 bytes.
            return (uint) ((sizeInBytes + 255) & ~255);
        }
    }
}
