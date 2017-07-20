using System;
using System.Runtime.InteropServices;
using Metal;
using OpenZH.Graphics.Platforms.iOS.BlockCompression;
using OpenZH.Graphics.Platforms.Metal;

namespace OpenZH.Graphics
{
    partial class Texture
    {
        private PixelFormat _originalPixelFormat;

        internal IMTLTexture DeviceTexture { get; private set; }

        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            PixelFormat pixelFormat,
            int width,
            int height,
            int numMipmapLevels)
        {
            var textureDescriptor = MTLTextureDescriptor.CreateTexture2DDescriptor(
                pixelFormat.ToMTLPixelFormat(),
                (nuint) width,
                (nuint) height,
                true); // Ignored, because we'll set the mip level count explicitly below.

            textureDescriptor.Usage = MTLTextureUsage.ShaderRead;

            textureDescriptor.MipmapLevelCount = (nuint) numMipmapLevels;

            _originalPixelFormat = pixelFormat;

            DeviceTexture = AddDisposable(graphicsDevice.Device.CreateTexture(textureDescriptor));
        }

        private void PlatformSetData(ResourceUploadBatch uploadBatch, int level, byte[] data, int bytesPerRow)
        {
            var processedData = data;
            var processedBytesPerRow = bytesPerRow;

            switch (_originalPixelFormat)
            {
                case PixelFormat.Bc1:
                case PixelFormat.Bc2:
                case PixelFormat.Bc3:
                    processedData = BlockCompressionUtility.Decompress(
                        _originalPixelFormat,
                        data,
                        bytesPerRow,
                        out processedBytesPerRow);
                    break;
            }

            var region = MTLRegion.Create2D(0, 0, DeviceTexture.Width, DeviceTexture.Height);

            var pinnedArray = GCHandle.Alloc(processedData, GCHandleType.Pinned);
            try
            {
                var dataPointer = pinnedArray.AddrOfPinnedObject();

                DeviceTexture.ReplaceRegion(
                    region,
                    (uint) level,
                    dataPointer,
                    (uint) processedBytesPerRow);
            }
            finally
            {
                pinnedArray.Free();
            }
        }
    }
}