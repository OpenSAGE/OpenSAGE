using System;
using System.Threading.Tasks;
using System.Windows;

namespace LLGfx.Hosting
{
    public class GraphicsDeviceControl : HwndWrapper, IGraphicsView
    {
        public event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        public event EventHandler<GraphicsEventArgs> GraphicsDraw;
        public event EventHandler GraphicsUninitialized;

        private Task _renderLoopTask;
        private bool _unloading;

        private SwapChain _swapChain;

        public GraphicsDevice GraphicsDevice { get; set; }

        public SwapChain SwapChain => _swapChain;

        public GraphicsDeviceControl()
        {
            Loaded += OnLoaded;
        }

        private void DoRenderLoop()
        {
            while (!_unloading)
            {
                System.Windows.Forms.Application.DoEvents();

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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_swapChain != null)
            {
                return;
            }

            _swapChain = new SwapChain(
                GraphicsDevice,
                Handle,
                3,
                Math.Max((int) ActualWidth, 1),
                Math.Max((int) ActualHeight, 1));

            GraphicsInitialize?.Invoke(this, new GraphicsEventArgs(GraphicsDevice, _swapChain));

            StartRenderLoop();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (_swapChain != null)
            {
                StopRenderLoop();

                _swapChain.Resize(
                    Math.Max((int) sizeInfo.NewSize.Width, 1),
                    Math.Max((int) sizeInfo.NewSize.Height, 1));

                StartRenderLoop();
            }
        }

        private void StartRenderLoop()
        {
            _unloading = false;
            _renderLoopTask = Task.Run(() => DoRenderLoop());
        }

        protected void StopRenderLoop()
        {
            _unloading = true;
            _renderLoopTask?.Wait();
            _renderLoopTask = null;
        }

        protected override void Dispose(bool disposing)
        {
            StopRenderLoop();

            _swapChain?.Dispose();
            _swapChain = null;

            GraphicsUninitialized?.Invoke(this, EventArgs.Empty);

            base.Dispose(disposing);
        }
    }
}
