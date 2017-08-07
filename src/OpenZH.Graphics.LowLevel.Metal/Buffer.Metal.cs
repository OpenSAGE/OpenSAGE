using Metal;

namespace OpenZH.Graphics.LowLevel
{
    partial class Buffer
    {
        internal IMTLBuffer DeviceBuffer { get; /* protected */ set; }

        private uint PlatformGetAlignedSize(uint sizeInBytes) => sizeInBytes;
    }
}