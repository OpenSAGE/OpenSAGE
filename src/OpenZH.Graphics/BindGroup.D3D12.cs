using System;
using OpenZH.Graphics.Platforms.Direct3D12;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class BindGroup
    {
        private DescriptorTablePoolEntry _cbvUavSrvPoolEntry;
        private DescriptorTablePoolEntry _samplerPoolEntry;

        // One of these will be non-null, but not both.
        internal GpuDescriptorHandle? GPUDescriptorHandleForCbvUavSrvHeapStart => _cbvUavSrvPoolEntry?.GpuDescriptorHandle;
        internal GpuDescriptorHandle? GPUDescriptorHandleForSamplerHeapStart => _samplerPoolEntry?.GpuDescriptorHandle;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, BindGroupLayout layout)
        {
            var numCbvUavSrv = 0;
            var numSamplers = 0;

            foreach (var binding in layout.Description.Bindings)
            {
                switch (binding.BindingType)
                {
                    case BindingType.ConstantBuffer:
                    case BindingType.Texture:
                        numCbvUavSrv++;
                        break;

                    case BindingType.Sampler:
                        numSamplers++;
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            if (numCbvUavSrv > 0)
            {
                _cbvUavSrvPoolEntry = graphicsDevice.DescriptorHeapCbvUavSrv.Reserve(numCbvUavSrv);
            }

            if (numSamplers > 0)
            {
                _samplerPoolEntry = graphicsDevice.DescriptorHeapSampler.Reserve(numSamplers);
            }
        }

        private void PlatformSetBuffer(int index, Buffer buffer)
        {
            GraphicsDevice.Device.CreateConstantBufferView(
                new ConstantBufferViewDescription
                {
                    BufferLocation = buffer.DeviceBuffer.GPUVirtualAddress,
                    SizeInBytes = (int) buffer.SizeInBytes
                },
                _cbvUavSrvPoolEntry.CpuDescriptorHandle + (_cbvUavSrvPoolEntry.IncrementSize * index));
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
                _cbvUavSrvPoolEntry.CpuDescriptorHandle + (_cbvUavSrvPoolEntry.IncrementSize * index));
        }
    }
}
