using System;
using System.Runtime.InteropServices;
using Metal;
using OpenSage.LowLevel.Graphics3D.Util;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class Texture
    {
        public IMTLTexture DeviceTexture { get; private set; }

        internal override string PlatformGetDebugName() => DeviceTexture.Label;
        internal override void PlatformSetDebugName(string value) => DeviceTexture.Label = value;

        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            PixelFormat pixelFormat,
            int arraySize,
            int width,
            int height,
            TextureBindFlags bindFlags,
            int mipMapCount,
            TextureMipMapData[] mipMapData)
        {
            var descriptor = new MTLTextureDescriptor
            {
                ArrayLength = (nuint) arraySize,
                CpuCacheMode = MTLCpuCacheMode.DefaultCache,
                Depth = 1,
                Height = (nuint) height,
                MipmapLevelCount = (nuint) mipMapCount,
                PixelFormat = pixelFormat.ToMTLPixelFormat(),
                ResourceOptions = MTLResourceOptions.StorageModeManaged,
                SampleCount = 1,
                StorageMode = MTLStorageMode.Managed,
                TextureType = arraySize > 1 ? MTLTextureType.k2DArray : MTLTextureType.k2D,
                Width = (nuint) width,
                Usage = bindFlags.ToMTLTextureUsage()
            };

            DeviceTexture = AddDisposable(graphicsDevice.Device.CreateTexture(descriptor));

            if (mipMapData != null)
            {
                for (var i = 0; i < mipMapData.Length; i++)
                {
                    var mipWidth = CalculateMipMapSize(i, width);
                    var mipHeight = CalculateMipMapSize(i, height);

                    var region = MTLRegion.Create2D(0, 0, mipWidth, mipHeight);

                    var dataHandle = GCHandle.Alloc(mipMapData[i].Data, GCHandleType.Pinned);
                    try
                    {
                        DeviceTexture.ReplaceRegion(
                            region,
                            (nuint) i,
                            dataHandle.AddrOfPinnedObject(),
                            (nuint) mipMapData[i].BytesPerRow);
                    }
                    finally
                    {
                        dataHandle.Free();
                    }
                }
            }
        }

        private void PlatformCopyFromTexture(
            Texture source,
            int destinationArrayIndex)
        {
            // TODO: Need to use MTLBlitCommandEncoder
            throw new NotImplementedException();
        }
    }
}
