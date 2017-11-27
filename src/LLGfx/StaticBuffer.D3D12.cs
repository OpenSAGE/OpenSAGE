using LLGfx.Util;
using SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace LLGfx
{
    partial class StaticBuffer<T>
    {
        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            T[] data,
            uint sizeInBytes,
            uint elementSizeInBytes,
            BufferBindFlags flags)
        {
            using (var dataStream = new DataStream((int) sizeInBytes, true, true))
            {
                dataStream.WriteRange(data);
                dataStream.Seek(0, System.IO.SeekOrigin.Begin);

                var optionFlags = flags.HasFlag(BufferBindFlags.ShaderResource)
                    ? D3D11.ResourceOptionFlags.BufferStructured
                    : D3D11.ResourceOptionFlags.None;

                DeviceBuffer = AddDisposable(new D3D11.Buffer(
                    graphicsDevice.Device,
                    dataStream,
                    new D3D11.BufferDescription
                    {
                        BindFlags = flags.ToBindFlags(),
                        CpuAccessFlags = D3D11.CpuAccessFlags.None,
                        OptionFlags = optionFlags,
                        SizeInBytes = (int) sizeInBytes,
                        StructureByteStride = (int) elementSizeInBytes,
                        Usage = D3D11.ResourceUsage.Immutable
                    }));
            }
        }
    }
}
