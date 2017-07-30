using System;
using OpenZH.Graphics.Platforms.Direct3D12;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class DescriptorSet
    {
        private DescriptorTablePoolEntry _cbvUavSrvPoolEntry;

        internal GpuDescriptorHandle GPUDescriptorHandleForCbvUavSrvHeapStart => _cbvUavSrvPoolEntry.GpuDescriptorHandle;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, DescriptorSetLayout layout)
        {
            _cbvUavSrvPoolEntry = graphicsDevice.DescriptorHeapCbvUavSrv.Reserve(layout.Description.Bindings.Length);
        }

        private void PlatformSetConstantBuffer(int index, StaticBuffer buffer)
        {
            GraphicsDevice.Device.CreateConstantBufferView(
                new ConstantBufferViewDescription
                {
                    BufferLocation = buffer.DeviceBuffer.GPUVirtualAddress,
                    SizeInBytes = (int) buffer.SizeInBytes
                },
                _cbvUavSrvPoolEntry.GetCpuHandle(index));
        }

        private void PlatformSetTexture(int index, Texture texture)
        {
            var deviceResource = texture.DeviceResource;

            var description = new ShaderResourceViewDescription
            {
                Shader4ComponentMapping = ComponentMappingUtility.DefaultComponentMapping(),
                Format = deviceResource.Description.Format
            };

            switch (deviceResource.Description.Dimension)
            {
                case ResourceDimension.Texture2D:
                    description.Dimension = ShaderResourceViewDimension.Texture2D;
                    description.Texture2D.MipLevels = -1;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            GraphicsDevice.Device.CreateShaderResourceView(
                deviceResource,
                description,
                _cbvUavSrvPoolEntry.GetCpuHandle(index));
        }
    }
}
