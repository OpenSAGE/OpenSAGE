using SharpDX.Direct3D12;

namespace LLGfx
{
    partial class Buffer
    {
        public const int ConstantBufferAlignment = 256;

        private const int ConstantBufferAlignmentMask = ConstantBufferAlignment - 1;

        internal abstract long DeviceCurrentGPUVirtualAddress { get; }

        private uint PlatformGetAlignedSize(uint sizeInBytes)
        {
            // Align to 256 bytes.
            return (uint) ((sizeInBytes + ConstantBufferAlignmentMask) & ~ConstantBufferAlignmentMask);
        }
    }
}
