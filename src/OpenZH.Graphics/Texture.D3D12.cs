using OpenZH.Graphics.Platforms.Direct3D12;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class Texture
    {
        private DescriptorHeap _descriptorHeap;

        internal Resource DeviceResource { get; private set; }

        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            PixelFormat pixelFormat,
            int width,
            int height,
            int numMipmapLevels)
        {
            var resourceDescription = ResourceDescription.Texture2D(
                pixelFormat.ToDxgiFormat(),
                width,
                height,
                mipLevels: (short) numMipmapLevels);

            DeviceResource = AddDisposable(graphicsDevice.Device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                resourceDescription,
                ResourceStates.CopyDestination));

            _descriptorHeap = AddDisposable(graphicsDevice.Device.CreateDescriptorHeap(
                new DescriptorHeapDescription
                {
                    DescriptorCount = 1,
                    Flags = DescriptorHeapFlags.ShaderVisible,
                    Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView
                }));

            graphicsDevice.Device.CreateShaderResourceView(
                DeviceResource,
                new ShaderResourceViewDescription
                {
                    Shader4ComponentMapping = ComponentMappingUtility.DefaultComponentMapping(),
                    Format = resourceDescription.Format,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D =
                    {
                        MipLevels = resourceDescription.MipLevels
                    }
                },
                _descriptorHeap.CPUDescriptorHandleForHeapStart);
        }

        private void PlatformSetData(ResourceUploadBatch uploadBatch, int level, byte[] data, int bytesPerRow)
        {
            uploadBatch.Upload(DeviceResource, level, data, bytesPerRow);

            uploadBatch.Transition(
                DeviceResource,
                level,
                ResourceStates.CopyDestination,
                ResourceStates.PixelShaderResource);
        }
    }
}
