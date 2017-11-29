using System;
using LLGfx.Util;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D3D11 = SharpDX.Direct3D11;

namespace LLGfx
{
    partial class Texture
    {
        internal Texture2D DeviceResource { get; private set; }

        private D3D11.ShaderResourceView _deviceShaderResourceView;
        internal D3D11.ShaderResourceView DeviceShaderResourceView
        {
            get
            {
                if (_deviceShaderResourceView == null)
                {
                    var description = new ShaderResourceViewDescription
                    {
                        Format = DeviceResource.Description.Format
                    };

                    switch (DeviceResource.Dimension)
                    {
                        case ResourceDimension.Texture2D:
                            if (DeviceResource.Description.ArraySize > 1)
                            {
                                description.Dimension = ShaderResourceViewDimension.Texture2DArray;
                                description.Texture2DArray.ArraySize = DeviceResource.Description.ArraySize;
                                description.Texture2DArray.FirstArraySlice = 0;
                                description.Texture2DArray.MipLevels = -1;
                            }
                            else
                            {
                                description.Dimension = ShaderResourceViewDimension.Texture2D;
                                description.Texture2D.MipLevels = -1;
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    _deviceShaderResourceView = AddDisposable(new D3D11.ShaderResourceView(
                        GraphicsDevice.Device,
                        DeviceResource,
                        description));
                }
                return _deviceShaderResourceView;
            }
        }

        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            PixelFormat pixelFormat,
            int arraySize,
            int width,
            int height,
            int mipMapCount,
            TextureMipMapData[] mipMapData)
        {
            var resourceDescription = new Texture2DDescription
            {
                ArraySize = arraySize,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = pixelFormat.ToDxgiFormat(),
                Height = height,
                MipLevels = mipMapCount,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = mipMapData != null ? D3D11.ResourceUsage.Immutable : D3D11.ResourceUsage.Default,
                Width = width
            };

            if (mipMapData != null)
            {
                var dataStreams = new DataStream[mipMapData.Length];
                var dataBoxes = new DataBox[mipMapData.Length];
                for (var i = 0; i < mipMapData.Length; i++)
                {
                    dataStreams[i] = DataStream.Create(mipMapData[i].Data, true, false);
                    dataBoxes[i] = new DataBox(dataStreams[i].DataPointer, mipMapData[i].BytesPerRow, 0);
                }
                
                DeviceResource = AddDisposable(new Texture2D(
                    graphicsDevice.Device,
                    resourceDescription,
                    dataBoxes));

                foreach (var dataStream in dataStreams)
                {
                    dataStream.Dispose();
                }
            }
            else
            {
                DeviceResource = AddDisposable(new Texture2D(
                    graphicsDevice.Device,
                    resourceDescription));
            }
        }

        private void PlatformCopyFromTexture(
            Texture source,
            int destinationArrayIndex)
        {
            var mipLevels = source.DeviceResource.Description.MipLevels;
            for (var i = 0; i < mipLevels; i++)
            {
                GraphicsDevice.Device.ImmediateContext.CopySubresourceRegion(
                    source.DeviceResource,
                    i,
                    null,
                    DeviceResource,
                    Resource.CalculateSubResourceIndex(i, destinationArrayIndex, mipLevels));
            }
        }
    }
}
