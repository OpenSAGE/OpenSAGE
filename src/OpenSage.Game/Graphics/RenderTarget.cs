using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics
{
    internal sealed class RenderTarget : DisposableBase
    {
        public static readonly OutputDescription OutputDescription = new OutputDescription(
            null,
            new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm));

        private readonly GraphicsDevice _graphicsDevice;

        private Size _size;
        private Texture _colorTarget;
        private Framebuffer _framebuffer;

        public Texture ColorTarget => _colorTarget;
        public Framebuffer Framebuffer => _framebuffer;

        public RenderTarget(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public bool EnsureSize(in Size size)
        {
            if (size == _size)
            {
                return false;
            }

            _size = size;

            RemoveAndDispose(ref _framebuffer);
            RemoveAndDispose(ref _colorTarget);

            var width = (uint) _size.Width;
            var height = (uint) _size.Height;

            _colorTarget = AddDisposable(_graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    width,
                    height,
                    1,
                    1,
                    PixelFormat.B8_G8_R8_A8_UNorm,
                    TextureUsage.RenderTarget | TextureUsage.Sampled)));

            _framebuffer = AddDisposable(_graphicsDevice.ResourceFactory.CreateFramebuffer(
                new FramebufferDescription(null, _colorTarget)));

            return true;
        }
    }
}
