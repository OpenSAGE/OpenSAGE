using System;
using System.Threading;
using OpenSage.LowLevel.Graphics3D.Util;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class SwapChain
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly int _backBufferCount;

        private RenderTarget _renderTarget;
        private RenderTargetView _renderTargetView;

        private int PlatformBackBufferWidth { get; set; }
        private int PlatformBackBufferHeight { get; set; }

        private DXGI.SwapChain DeviceSwapChain { get; }

        public SwapChain(
            GraphicsDevice graphicsDevice,
            IntPtr windowHandle,
            int backBufferCount,
            int width,
            int height)
        {
            _graphicsDevice = graphicsDevice;
            _backBufferCount = backBufferCount;

            DeviceSwapChain = AddDisposable(CreateSwapChain(
                windowHandle,
                _graphicsDevice.Device,
                _graphicsDevice.BackBufferFormat,
                backBufferCount,
                width,
                height));

            Resize(width, height);
        }

        private static DXGI.SwapChain CreateSwapChain(
            IntPtr windowHandle,
            D3D11.Device device,
            PixelFormat backBufferFormat,
            int backBufferCount,
            int width,
            int height)
        {
            var swapChainDescription = new SwapChainDescription
            {
                IsWindowed = true,
                ModeDescription = new ModeDescription(width, height, new Rational(60, 0), backBufferFormat.ToDxgiFormat()),
                OutputHandle = windowHandle,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = backBufferCount,
                SwapEffect = SwapEffect.Discard,
                Flags = SwapChainFlags.None
            };

            using (var dxgiFactory = new Factory1())
            {
                return new DXGI.SwapChain(dxgiFactory, device, swapChainDescription);
            }
        }

        public void Resize(int newWidth, int newHeight)
        {
            // Clear the previous window size specific content and update the tracked fence values.
            RemoveAndDispose(ref _renderTarget);
            RemoveAndDispose(ref _renderTargetView);

            DeviceSwapChain.ResizeBuffers(
                _backBufferCount,
                newWidth,
                newHeight,
                _graphicsDevice.BackBufferFormat.ToDxgiFormat(),
                SwapChainFlags.None);

            using (var backBuffer = DeviceSwapChain.GetBackBuffer<Texture2D>(0))
            {
                _renderTargetView = AddDisposable(new RenderTargetView(
                    _graphicsDevice.Device,
                    backBuffer));
            }

            _renderTarget = AddDisposable(new RenderTarget(
                _graphicsDevice,
                _renderTargetView));

            PlatformBackBufferWidth = newWidth;
            PlatformBackBufferHeight = newHeight;
        }

        private RenderTarget PlatformGetNextRenderTarget()
        {
            return _renderTarget;
        }

        internal void Present()
        {
            DeviceSwapChain.Present(1, PresentFlags.None);
        }
    }
}
