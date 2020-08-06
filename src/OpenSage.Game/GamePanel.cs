using System;
using OpenSage.Graphics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage
{
    public sealed class GamePanel : DisposableBase
    {
        private readonly RenderTarget _renderTarget;

        public Framebuffer Framebuffer => _renderTarget.Framebuffer;

        public OutputDescription OutputDescription { get; } = RenderTarget.OutputDescription;

        public event EventHandler ClientSizeChanged;

        public Rectangle Frame { get; private set; }

        public Rectangle ClientBounds => new Rectangle(0, 0, Frame.Width, Frame.Height);

        public void SetCursor(Cursor cursor)
        {
            // TODO
        }

        internal GamePanel(GraphicsDevice graphicsDevice)
        {
            _renderTarget = AddDisposable(new RenderTarget(graphicsDevice));
        }

        public void EnsureFrame(in Rectangle frame)
        {
            if (frame == Frame)
            {
                return;
            }

            Frame = frame;

            if (_renderTarget.EnsureSize(frame.Size))
            {
                ClientSizeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
