using System;
using OpenZH.Graphics.Direct3D12;
using SharpDX;
using SharpDX.DXGI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace OpenZH.Platform.UWP
{
    public class D3D12SwapChainPanel : SwapChainPanel, IGraphicsView
    {
        public event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        public event EventHandler<GraphicsDrawEventArgs> GraphicsDraw;

        private readonly D3D12GraphicsDevice _graphicsDevice;
        private readonly D3D12SwapChain _swapChain;

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
            _graphicsDevice = new D3D12GraphicsDevice();

            _swapChain = new D3D12SwapChain(
                _graphicsDevice.Device,
                ((D3D12CommandQueue) _graphicsDevice.CommandQueue).DeviceCommandQueue,
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

            GraphicsInitialize?.Invoke(this, new GraphicsEventArgs(_graphicsDevice));

            if (!RedrawsOnTimer)
                Draw();

            Loaded -= OnLoaded;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _swapChain.Resize((int) e.NewSize.Width, (int) e.NewSize.Height);

            if (!RedrawsOnTimer)
                Draw();
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
            GraphicsDraw?.Invoke(this, new GraphicsDrawEventArgs(_graphicsDevice, _swapChain));
        }

        private void OnRendering(object sender, object e)
        {
            Draw();
        }
    }
}
