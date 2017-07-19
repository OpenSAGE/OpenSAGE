using System;
using Metal;

namespace OpenZH.Graphics.Metal
{
    public sealed class MetalGraphicsDevice : GraphicsDevice
    {
        public IMTLDevice Device { get; }

        public override CommandQueue CommandQueue { get; }

        public MetalGraphicsDevice()
        {
            Device = MTLDevice.SystemDefault;

            CommandQueue = new MetalCommandQueue(Device);
        }

        public override RenderPassDescriptor CreateRenderPassDescriptor()
        {
            return new MetalRenderPassDescriptor();
        }

        public override ResourceUploadBatch CreateResourceUploadBatch()
        {
            return new MetalResourceUploadBatch();
        }

        public override Texture CreateTexture2D(PixelFormat pixelFormat, int width, int height, int numMipmapLevels)
        {
            var textureDescriptor = MTLTextureDescriptor.CreateTexture2DDescriptor(
                pixelFormat.ToMTLPixelFormat(),
                (nuint) width,
                (nuint) height,
                true); // Ignored, because we'll set the mip level count explicitly below.

            textureDescriptor.Usage = MTLTextureUsage.ShaderRead;

            textureDescriptor.MipmapLevelCount = (nuint) numMipmapLevels;

            return new MetalTexture(Device, textureDescriptor, pixelFormat);
        }
    }
}