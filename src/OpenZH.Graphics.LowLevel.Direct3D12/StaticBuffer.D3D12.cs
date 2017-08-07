using SharpDX.Direct3D12;

namespace OpenZH.Graphics.LowLevel
{
    partial class StaticBuffer
    {
        internal override long DeviceCurrentGPUVirtualAddress => DeviceBuffer.GPUVirtualAddress;

        private void PlatformConstruct<T>(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            T[] data,
            uint sizeInBytes)
            where T : struct
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
