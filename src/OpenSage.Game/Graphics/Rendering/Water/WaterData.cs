using Veldrid;

namespace OpenSage.Graphics.Rendering.Water
{
    internal sealed class WaterData : DisposableBase
    {
        public const PixelFormat ReflectionMapPixelFormat = PixelFormat.B8_G8_R8_A8_UNorm;
        public const PixelFormat ReflectionDepthMapPixelFormat = PixelFormat.D24_UNorm_S8_UInt;

        public const PixelFormat RefractionMapPixelFormat = PixelFormat.B8_G8_R8_A8_UNorm;
        public const PixelFormat RefractionDepthMapPixelFormat = PixelFormat.D24_UNorm_S8_UInt;

        public uint ReflectionMapSize { get; }

        public Texture ReflectionMap { get; }
        public Texture ReflectionDepthMap { get; }
        public Framebuffer ReflectionMapFramebuffer { get; }

        public uint RefractionMapSize { get; }
        public Texture RefractionMap { get; }
        public Texture RefractionDepthMap { get; }
        public Framebuffer RefractionMapFramebuffer { get; }

        public WaterData(
            uint reflectionMapSize,
            uint refractionMapSize,
            GraphicsDevice graphicsDevice)
        {
            ReflectionMapSize = reflectionMapSize;
            RefractionMapSize = refractionMapSize;

            ReflectionMap = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    reflectionMapSize,
                    reflectionMapSize,
                    1,
                    1,
                    ReflectionMapPixelFormat,
                    TextureUsage.RenderTarget | TextureUsage.Sampled)));
            ReflectionDepthMap = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    reflectionMapSize,
                    reflectionMapSize,
                    1,
                    1,
                    ReflectionDepthMapPixelFormat,
                    TextureUsage.DepthStencil | TextureUsage.Sampled)));
            ReflectionMapFramebuffer = AddDisposable(graphicsDevice.ResourceFactory.CreateFramebuffer(
                new FramebufferDescription(
                    new FramebufferAttachmentDescription(ReflectionDepthMap, 0),
                    new FramebufferAttachmentDescription[] { new FramebufferAttachmentDescription(ReflectionMap, 0) })));

            RefractionMap = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    refractionMapSize,
                    refractionMapSize,
                    1,
                    1,
                    RefractionMapPixelFormat,
                    TextureUsage.RenderTarget | TextureUsage.Sampled)));
            RefractionDepthMap = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    refractionMapSize,
                    refractionMapSize,
                    1,
                    1,
                    RefractionDepthMapPixelFormat,
                    TextureUsage.DepthStencil | TextureUsage.Sampled)));
            RefractionMapFramebuffer = AddDisposable(graphicsDevice.ResourceFactory.CreateFramebuffer(
                new FramebufferDescription(
                    new FramebufferAttachmentDescription(RefractionDepthMap, 0),
                    new FramebufferAttachmentDescription[] { new FramebufferAttachmentDescription(RefractionMap, 0) })));

        }
    }
}
