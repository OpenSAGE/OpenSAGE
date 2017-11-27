using LLGfx.Util;
using D3D11 = SharpDX.Direct3D11;

namespace LLGfx
{
    partial class DynamicBuffer<T>
    {
        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            uint sizeInBytes,
            uint elementSizeInBytes,
            BufferBindFlags flags)
        {
            var optionFlags = flags.HasFlag(BufferBindFlags.ShaderResource)
                ? D3D11.ResourceOptionFlags.BufferStructured
                : D3D11.ResourceOptionFlags.None;

            DeviceBuffer = AddDisposable(new D3D11.Buffer(
                graphicsDevice.Device,
                new D3D11.BufferDescription
                {
                    BindFlags = flags.ToBindFlags(),
                    CpuAccessFlags = D3D11.CpuAccessFlags.Write,
                    OptionFlags = optionFlags,
                    SizeInBytes = (int) sizeInBytes,
                    StructureByteStride = (int) elementSizeInBytes,
                    Usage = D3D11.ResourceUsage.Dynamic
                }));
        }

        private void PlatformSetData(T[] data)
        {
            var dataBox = GraphicsDevice.Device.ImmediateContext.MapSubresource(
                DeviceBuffer,
                D3D11.MapMode.WriteDiscard,
                D3D11.MapFlags.None,
                out var dataStream);

            dataStream.WriteRange(data);

            GraphicsDevice.Device.ImmediateContext.UnmapSubresource(DeviceBuffer, 0);

            dataStream.Dispose();
        }

        private void PlatformSetData(ref T data)
        {
            var dataBox = GraphicsDevice.Device.ImmediateContext.MapSubresource(
                DeviceBuffer,
                D3D11.MapMode.WriteDiscard,
                D3D11.MapFlags.None,
                out var dataStream);

            dataStream.Write(data);

            GraphicsDevice.Device.ImmediateContext.UnmapSubresource(DeviceBuffer, 0);

            dataStream.Dispose();
        }
    }
}
