using System;
using SharpDX;
using SharpDX.DXGI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace OpenZH.Graphics.Hosting.Direct3D12
{
    public class D3D12SwapChainPanel : SwapChainPanel, IGraphicsView
    {
        public event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        public event EventHandler<GraphicsEventArgs> GraphicsDraw;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly SwapChain _swapChain;

        private bool _redrawsOnTimer;

        public bool RedrawsOnTimer
        {
            get { return _redrawsOnTimer; }
            set
            {
                if (value == _redrawsOnTimer)
                {
                    return;
                }

                if (_redrawsOnTimer)
                {
                    CompositionTarget.Rendering -= OnRendering;
                }

                if (value)
                {
                    CompositionTarget.Rendering += OnRendering;
                }

                _redrawsOnTimer = true;
            }
        }

        public D3D12SwapChainPanel()
        {
            _graphicsDevice = new GraphicsDevice();

            _swapChain = new SwapChain(
                _graphicsDevice,
                3,
                1,
                1);

            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var nativePanel = ComObject.As<ISwapChainPanelNative>(this);
            nativePanel.SwapChain = _swapChain.DeviceSwapChain;

            GraphicsInitialize?.Invoke(this, new GraphicsEventArgs(_graphicsDevice, _swapChain));

            if (!RedrawsOnTimer)
            {
                Draw();
            }

            Loaded -= OnLoaded;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _swapChain.Resize((int) e.NewSize.Width, (int) e.NewSize.Height);

            if (!RedrawsOnTimer)
            {
                Draw();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_redrawsOnTimer)
            {
                CompositionTarget.Rendering -= OnRendering;
            }
        }

        public void Draw()
        {
            GraphicsDraw?.Invoke(this, new GraphicsEventArgs(_graphicsDevice, _swapChain));
        }

        private void OnRendering(object sender, object e)
        {
            Draw();
        }
    }
}
