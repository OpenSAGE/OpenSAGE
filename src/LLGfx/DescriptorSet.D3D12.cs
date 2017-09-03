using System;
using SharpDX.Direct3D12;
using LLGfx.Util;

namespace LLGfx
{
    partial class DescriptorSet
    {
        private DescriptorTablePoolEntry _cbvUavSrvPoolEntry;

        internal GpuDescriptorHandle GPUDescriptorHandleForCbvUavSrvHeapStart => _cbvUavSrvPoolEntry.GpuDescriptorHandle;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, DescriptorSetLayout layout)
        {
            _cbvUavSrvPoolEntry = graphicsDevice.DescriptorHeapCbvUavSrv.Reserve(layout.Description.NumBindings);
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

        private void PlatformSetStructuredBuffer(int index, StaticBuffer buffer)
        {
            var deviceResource = buffer.DeviceBuffer;

            var description = new ShaderResourceViewDescription
            {
                Shader4ComponentMapping = ComponentMappingUtility.DefaultComponentMapping(),
                Format = SharpDX.DXGI.Format.Unknown,
                Dimension = ShaderResourceViewDimension.Buffer,
                Buffer =
                {
                    FirstElement = 0,
                    ElementCount = (int) buffer.ElementCount,
                    StructureByteStride = (int) buffer.ElementSizeInBytes,
                    Flags = BufferShaderResourceViewFlags.None
                }
            };

            GraphicsDevice.Device.CreateShaderResourceView(
                deviceResource,
                description,
                _cbvUavSrvPoolEntry.GetCpuHandle(index));
        }

        private void PlatformSetTypedBuffer(int index, StaticBuffer buffer, PixelFormat format)
        {
            var deviceResource = buffer.DeviceBuffer;

            var description = new ShaderResourceViewDescription
            {
                Shader4ComponentMapping = ComponentMappingUtility.DefaultComponentMapping(),
                Format = format.ToDxgiFormat(),
                Dimension = ShaderResourceViewDimension.Buffer,
                Buffer =
                {
                    FirstElement = 0,
                    ElementCount = (int) buffer.ElementCount,
                    Flags = BufferShaderResourceViewFlags.None
                }
            };

            GraphicsDevice.Device.CreateShaderResourceView(
                deviceResource,
                description,
                _cbvUavSrvPoolEntry.GetCpuHandle(index));
        }

        private void PlatformSetTexture(int index, Texture texture)
        {
            var deviceResource = texture?.DeviceResource;
            var format = deviceResource?.Description.Format ?? SharpDX.DXGI.Format.BC1_UNorm;
            var dimension = deviceResource?.Description.Dimension ?? ResourceDimension.Texture2D;

            var description = new ShaderResourceViewDescription
            {
                Shader4ComponentMapping = ComponentMappingUtility.DefaultComponentMapping(),
                Format = format
            };

            switch (dimension)
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
