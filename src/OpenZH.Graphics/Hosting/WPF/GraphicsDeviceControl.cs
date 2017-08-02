using System;
using System.Windows;

namespace OpenZH.Graphics.Hosting.Windows
{
    public class GraphicsDeviceControl : HwndWrapper, IGraphicsView
    {
        public event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        public event EventHandler<GraphicsEventArgs> GraphicsDraw;

        private readonly GraphicsDevice _graphicsDevice;
        private SwapChain _swapChain;

        public GraphicsDeviceControl()
        {
            Loaded += OnLoaded;
        }

        protected override void Dispose(bool disposing)
        {
            Loaded -= OnLoaded;

            base.Dispose(disposing);
        }

        protected override void Draw()
        {
            if (_swapChain == null)
            {
                return;
            }

            GraphicsDraw?.Invoke(this, new GraphicsEventArgs(_graphicsDevice, _swapChain));
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_swapChain != null)
            {
                return;
            }

            _swapChain = new SwapChain(
                _graphicsDevice,
                Handle,
                3,
                (int) ActualWidth,
                (int) ActualHeight);

            if (!RedrawsOnTimer)
            {
                Draw();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            _swapChain.Resize(
                (int) sizeInfo.NewSize.Width,
                (int) sizeInfo.NewSize.Height);

            if (!RedrawsOnTimer)
            {
                Draw();
            }
        }

        void IGraphicsView.Draw() => Draw();
    }
}
