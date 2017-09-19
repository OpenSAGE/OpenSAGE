using System;
using LLGfx.Util;
using SharpDX.Direct3D12;

namespace LLGfx
{
    partial class ShaderResourceView
    {
        private DescriptorTablePoolEntry _cbvUavSrvPoolEntry;

        internal GpuDescriptorHandle GPUDescriptorHandleForCbvUavSrvHeapStart => _cbvUavSrvPoolEntry.GpuDescriptorHandle;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int numResources)
        {
            _cbvUavSrvPoolEntry = graphicsDevice.DescriptorHeapCbvUavSrv.Reserve(numResources);
        }

        private void PlatformSetStructuredBuffer<T>(int index, StaticBuffer<T> buffer)
            where T : struct
        {
            var deviceResource = buffer?.DeviceBuffer;

            var description = new ShaderResourceViewDescription
            {
                Shader4ComponentMapping = ComponentMappingUtility.DefaultComponentMapping(),
                Format = buffer != null
                    ? SharpDX.DXGI.Format.Unknown
                    : SharpDX.DXGI.Format.R32_UInt,
                Dimension = ShaderResourceViewDimension.Buffer,
                Buffer =
                {
                    FirstElement = 0,
                    ElementCount = (int) (buffer?.ElementCount ?? 0),
                    StructureByteStride = (int) (buffer?.ElementSizeInBytes ?? 0),
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

        protected override void Dispose(bool disposing)
        {
            GraphicsDevice.DescriptorHeapCbvUavSrv.Release(_cbvUavSrvPoolEntry);

            base.Dispose(disposing);
        }
    }
}
