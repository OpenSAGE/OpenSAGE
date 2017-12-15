using System;
using Metal;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class DepthStencilBuffer : GraphicsDeviceChild
    {
        internal IMTLTexture DeviceTexture { get; private set; }

        internal override string PlatformGetDebugName() => DeviceTexture.Label;
        internal override void PlatformSetDebugName(string value) => DeviceTexture.Label = value;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, float clearValue)
        {
            DeviceTexture = AddDisposable(graphicsDevice.Device.CreateTexture(new MTLTextureDescriptor
            {
                ArrayLength = 1,
                CpuCacheMode = MTLCpuCacheMode.DefaultCache,
                Depth = 1,
                Height = (nuint) height,
                MipmapLevelCount = 1,
                PixelFormat = MTLPixelFormat.Depth32Float,
                ResourceOptions = MTLResourceOptions.StorageModePrivate,
                SampleCount = 1,
                StorageMode = MTLStorageMode.Private,
                TextureType = MTLTextureType.k2D,
                Usage = MTLTextureUsage.RenderTarget,
                Width = (nuint) width
            }));
        }
    }
}
