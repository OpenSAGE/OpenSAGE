using Metal;

namespace OpenZH.Graphics.Metal
{
    public sealed class MetalBuffer : Buffer
    {
        public IMTLBuffer DeviceBuffer { get; }
    }
}