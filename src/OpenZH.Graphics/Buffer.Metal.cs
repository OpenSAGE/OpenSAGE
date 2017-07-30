using Metal;

namespace OpenZH.Graphics
{
    partial class Buffer
    {
        internal IMTLBuffer DeviceBuffer { get; /* protected */ set; }

        private uint PlatformGetAlignedSize(uint sizeInBytes) => sizeInBytes;
    }
}