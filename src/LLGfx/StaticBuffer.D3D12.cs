using SharpDX.Direct3D12;

namespace LLGfx
{
    partial class StaticBuffer<T>
    {
        internal Resource DeviceBuffer { get; private set; }

        internal override long DeviceCurrentGPUVirtualAddress => DeviceBuffer.GPUVirtualAddress;

        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            T[] data,
            uint sizeInBytes)
        {
            DeviceBuffer = AddDisposable(graphicsDevice.Device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                ResourceDescription.Buffer(SizeInBytes),
                ResourceStates.CopyDestination));

            uploadBatch.Upload(
                DeviceBuffer,
                new[]
                {
                    new ResourceUploadData<T>
                    {
                        Data = data,
                        BytesPerRow = (int) sizeInBytes
                    }
                });

            uploadBatch.Transition(
                DeviceBuffer,
                ResourceStates.CopyDestination,
                ResourceStates.VertexAndConstantBuffer);
        }
    }
}
