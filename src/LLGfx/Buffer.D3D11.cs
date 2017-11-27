using D3D11 = SharpDX.Direct3D11;

namespace LLGfx
{
    partial class Buffer
    {
        public const int ConstantBufferAlignment = 256;

        private const int ConstantBufferAlignmentMask = ConstantBufferAlignment - 1;

        internal D3D11.Buffer DeviceBuffer { get; set; }

        private D3D11.ShaderResourceView _deviceShaderResourceView;
        internal D3D11.ShaderResourceView DeviceShaderResourceView
        {
            get
            {
                if (_deviceShaderResourceView == null)
                {
                    _deviceShaderResourceView = AddDisposable(new D3D11.ShaderResourceView(
                        GraphicsDevice.Device,
                        DeviceBuffer,
                        new D3D11.ShaderResourceViewDescription
                        {
                            Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Buffer,
                            Format = SharpDX.DXGI.Format.Unknown,
                            Buffer =
                            {
                                FirstElement = 0,
                                ElementCount = (int) ElementCount
                            }
                        }));
                }
                return _deviceShaderResourceView;
            }
        }

        private uint PlatformGetAlignedSize(uint sizeInBytes)
        {
            // Align to 256 bytes.
            return (uint) ((sizeInBytes + ConstantBufferAlignmentMask) & ~ConstantBufferAlignmentMask);
        }
    }
}
