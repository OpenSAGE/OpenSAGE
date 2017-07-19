using System.Runtime.InteropServices;
using Metal;
using OpenZH.Graphics.Metal.Util.BlockCompression;

namespace OpenZH.Graphics.Metal
{
    public sealed class MetalTexture : Texture
    {
        private readonly PixelFormat _originalPixelFormat;

        public IMTLTexture Texture { get; }

        internal MetalTexture(IMTLDevice device, MTLTextureDescriptor textureDescriptor, PixelFormat originalPixelFormat)
        {
            _originalPixelFormat = originalPixelFormat;

            Texture = AddDisposable(device.CreateTexture(textureDescriptor));
        }

        public override void SetData(ResourceUploadBatch uploadBatch, int level, byte[] data, int bytesPerRow)
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

            var region = MTLRegion.Create2D(0, 0, Texture.Width, Texture.Height);

            var pinnedArray = GCHandle.Alloc(processedData, GCHandleType.Pinned);
            try
            {
                var dataPointer = pinnedArray.AddrOfPinnedObject();

                Texture.ReplaceRegion(
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