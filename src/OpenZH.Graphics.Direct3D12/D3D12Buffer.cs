using SharpDX.Direct3D12;

namespace OpenZH.Graphics.Direct3D12
{
    public sealed class D3D12Buffer : Buffer
    {
        public SharpDX.Direct3D12.Resource Buffer { get; }

        public D3D12Buffer(Device device, uint sizeInBytes)
        {
            Buffer = device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(sizeInBytes),
                ResourceStates.GenericRead);
        }
    }
}
