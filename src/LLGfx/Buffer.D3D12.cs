using SharpDX.Direct3D12;

namespace LLGfx
{
    partial class Buffer
    {
        internal abstract long DeviceCurrentGPUVirtualAddress { get; }

        private uint PlatformGetAlignedSize(uint sizeInBytes)
        {
            // Align to 256 bytes.
            return (uint) ((sizeInBytes + 255) & ~255);
        }
    }
}
