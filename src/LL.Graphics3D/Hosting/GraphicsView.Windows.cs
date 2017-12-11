using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LL.Graphics3D.Hosting
{
    public sealed class GraphicsView : Control
    {
        public event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        public event EventHandler<GraphicsEventArgs> GraphicsDraw;
        public event EventHandler GraphicsResized;
        public event EventHandler GraphicsUninitialized;

        private Task _renderLoopTask;
        private bool _unloading;

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

            StartRenderLoop();

            base.OnHandleCreated(e);
        }

        private void DoRenderLoop()
        {
            while (!_unloading)
            {
                Application.DoEvents();

                if (!_unloading)
                {
                    Draw();
                }
            }
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
                StopRenderLoop();

                _swapChain.Resize(
                    Math.Max(Width, 1),
                    Math.Max(Height, 1));

                GraphicsResized?.Invoke(this, EventArgs.Empty);

                StartRenderLoop();
            }

            base.OnSizeChanged(e);
        }

        private void StartRenderLoop()
        {
            _unloading = false;
            _renderLoopTask = Task.Run(() => DoRenderLoop());
        }

        private void StopRenderLoop()
        {
            _unloading = true;
            _renderLoopTask?.Wait();
            _renderLoopTask = null;
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            StopRenderLoop();

            _swapChain?.Dispose();
            _swapChain = null;

            GraphicsUninitialized?.Invoke(this, EventArgs.Empty);

            base.OnHandleDestroyed(e);
        }
    }
}
