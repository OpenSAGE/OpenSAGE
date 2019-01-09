using System;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage
{
    public sealed class GamePanel : DisposableBase
    {
        private Texture _gameColorTarget;
        private Framebuffer _gameFramebuffer;

        public GraphicsDevice GraphicsDevice { get; }

        public Framebuffer Framebuffer => _gameFramebuffer;

        public OutputDescription OutputDescription { get; } = new OutputDescription(
            null,
            new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm));

        public event EventHandler ClientSizeChanged;

        public Rectangle Frame { get; private set; }

        public Rectangle ClientBounds => new Rectangle(0, 0, Frame.Width, Frame.Height);

        public void SetCursor(Cursor cursor)
        {
            // TODO
        }

        internal GamePanel(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }

        public void EnsureFrame(in Rectangle frame)
        {
            if (frame == Frame)
            {
                return;
            }

            Frame = frame;

            RemoveAndDispose(ref _gameFramebuffer);
            RemoveAndDispose(ref _gameColorTarget);

            var width = (uint) Frame.Width;
            var height = (uint) Frame.Height;

            _gameColorTarget = AddDisposable(GraphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    width,
                    height,
                    1,
                    1,
                    PixelFormat.B8_G8_R8_A8_UNorm,
                    TextureUsage.RenderTarget | TextureUsage.Sampled)));

            _gameFramebuffer = AddDisposable(GraphicsDevice.ResourceFactory.CreateFramebuffer(
                new FramebufferDescription(null, _gameColorTarget)));

            ClientSizeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
