using System;
using System.Windows.Forms;
using SharpDX.Windows;

namespace LL.Graphics3D.Hosting
{
    public sealed class GraphicsView : RenderControl
    {
        public event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        public event EventHandler<GraphicsEventArgs> GraphicsDraw;
        public event EventHandler GraphicsResized;
        public event EventHandler GraphicsUninitialized;

        private SwapChain _swapChain;

        public GraphicsDevice GraphicsDevice { get; set; }

        public SwapChain SwapChain => _swapChain;

        protected override void OnHandleCreated(EventArgs e)
        {
            if (_swapChain != null)
            {
                return;
            }

            _swapChain = new SwapChain(
                GraphicsDevice,
                Handle,
                3,
                Math.Max(Width, 1),
                Math.Max(Height, 1));

            GraphicsInitialize?.Invoke(this, new GraphicsEventArgs(GraphicsDevice, _swapChain));

            base.OnHandleCreated(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Draw();

            base.OnPaint(e);

            Invalidate();
        }

        private void Draw()
        {
            if (_swapChain == null)
            {
                return;
            }

            GraphicsDraw?.Invoke(this, new GraphicsEventArgs(GraphicsDevice, _swapChain));
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (_swapChain != null)
            {
                _swapChain.Resize(
                    Math.Max(Width, 1),
                    Math.Max(Height, 1));

                GraphicsResized?.Invoke(this, EventArgs.Empty);
            }

            base.OnSizeChanged(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _swapChain?.Dispose();
            _swapChain = null;

            GraphicsUninitialized?.Invoke(this, EventArgs.Empty);

            base.OnHandleDestroyed(e);
        }
    }
}
