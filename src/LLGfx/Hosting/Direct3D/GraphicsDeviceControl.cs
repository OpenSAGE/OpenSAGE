using System;
using System.Windows;

namespace LLGfx.Hosting
{
    public class GraphicsDeviceControl : HwndWrapper, IGraphicsView
    {
        public event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        public event EventHandler<GraphicsEventArgs> GraphicsDraw;

        private SwapChain _swapChain;

        public GraphicsDevice GraphicsDevice { get; set; }

        public SwapChain SwapChain => _swapChain;

        public GraphicsDeviceControl()
        {
            Loaded += OnLoaded;
        }

        protected override void Dispose(bool disposing)
        {
            Loaded -= OnLoaded;

            if (_swapChain != null)
            {
                _swapChain.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void Draw()
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
                (int) ActualWidth,
                (int) ActualHeight);

            OnSwapChainResized(_swapChain);

            GraphicsInitialize?.Invoke(this, new GraphicsEventArgs(GraphicsDevice, _swapChain));

            if (!RedrawsOnTimer)
            {
                Draw();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (_swapChain != null)
            {
                _swapChain.Resize(
                    (int) sizeInfo.NewSize.Width,
                    (int) sizeInfo.NewSize.Height);

                OnSwapChainResized(_swapChain);

                if (!RedrawsOnTimer)
                {
                    Draw();
                }
            }
        }

        protected virtual void OnSwapChainResized(SwapChain newSwapChain)
        {

        }

        void IGraphicsView.Draw() => Draw();
    }
}
