using System;
using System.Threading;
using LLGfx.Util;
using SharpDX.Direct3D12;
using SharpDX.DXGI;
using D3D12 = SharpDX.Direct3D12;

namespace LLGfx
{
    partial class SwapChain
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly int _backBufferCount;

        private readonly D3D12.Resource[] _renderTargets;
        private readonly DescriptorHeap _descriptorHeap;
        private readonly int _descriptorSize;

        private int _frameIndex;
        private readonly AutoResetEvent _fenceEvent;
        private Fence _fence;
        private readonly int[] _fenceValues;

        private int PlatformBackBufferWidth { get; set; }
        private int PlatformBackBufferHeight { get; set; }

        public SwapChain3 DeviceSwapChain { get; }

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
                _graphicsDevice.CommandQueue.DeviceCommandQueue,
                _graphicsDevice.BackBufferFormat,
                backBufferCount,
                width,
                height));

            _renderTargets = new D3D12.Resource[backBufferCount];

            _descriptorHeap = AddDisposable(_graphicsDevice.Device.CreateDescriptorHeap(new DescriptorHeapDescription
            {
                DescriptorCount = backBufferCount,
                Flags = DescriptorHeapFlags.None,
                Type = DescriptorHeapType.RenderTargetView
            }));

            _descriptorSize = _graphicsDevice.Device.GetDescriptorHandleIncrementSize(
                DescriptorHeapType.RenderTargetView);

            _fence = AddDisposable(_graphicsDevice.Device.CreateFence(0, FenceFlags.None));

            _fenceValues = new int[backBufferCount];
            _fenceValues[_frameIndex]++;

            _fenceEvent = AddDisposable(new AutoResetEvent(false));

            Resize(width, height);
        }

        private static SwapChain3 CreateSwapChain(
            IntPtr windowHandle,
            D3D12.CommandQueue commandQueue,
            PixelFormat backBufferFormat,
            int backBufferCount,
            int width,
            int height)
        {
            var swapChainDescription = new SwapChainDescription1
            {
                Width = width,
                Height = height,
                Format = backBufferFormat.ToDxgiFormat(),
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                Scaling = Scaling.Stretch,
                BufferCount = backBufferCount,
                SwapEffect = SwapEffect.FlipSequential,
                Flags = SwapChainFlags.None
            };

#if DEBUG
            const bool debug = true;
#else
            const bool debug = false;
#endif
            using (var dxgiFactory = new Factory2(debug))
            using (var tempSwapChain = new SwapChain1(dxgiFactory, commandQueue, windowHandle, ref swapChainDescription))
            {
                return tempSwapChain.QueryInterface<SwapChain3>();
            }
        }

        public void Resize(int newWidth, int newHeight)
        {
            // Wait until all previous GPU work is complete.
            WaitForGpu();

            // Clear the previous window size specific content and update the tracked fence values.
            for (var i = 0; i < _backBufferCount; i++)
            {
                _renderTargets[i]?.Dispose();
                _renderTargets[i] = null;

                _fenceValues[i] = _fenceValues[_frameIndex];
            }

            DeviceSwapChain.ResizeBuffers(
                _backBufferCount,
                newWidth,
                newHeight,
                _graphicsDevice.BackBufferFormat.ToDxgiFormat(),
                SwapChainFlags.None);

            _frameIndex = DeviceSwapChain.CurrentBackBufferIndex;

            var handle = _descriptorHeap.CPUDescriptorHandleForHeapStart;
            for (var n = 0; n < _backBufferCount; n++)
            {
                _renderTargets[n] = DeviceSwapChain.GetBackBuffer<D3D12.Resource>(n);

                _graphicsDevice.Device.CreateRenderTargetView(
                    _renderTargets[n],
                    null,
                    handle);

                handle += _descriptorSize;
            }

            PlatformBackBufferWidth = newWidth;
            PlatformBackBufferHeight = newHeight;
        }

        private void WaitForGpu()
        {
            // Schedule a Signal command in the queue.
            _graphicsDevice.CommandQueue.DeviceCommandQueue.Signal(_fence, _fenceValues[_frameIndex]);

            // Wait until the fence has been crossed.
            _fence.SetEventOnCompletion(
                _fenceValues[_frameIndex], 
                _fenceEvent.GetSafeWaitHandle().DangerousGetHandle());
            _fenceEvent.WaitOne();

            // Increment the fence value for the current frame.
            _fenceValues[_frameIndex]++;
        }

        private RenderTarget PlatformGetNextRenderTarget()
        {
            return new RenderTarget(
                _graphicsDevice,
                _renderTargets[_frameIndex],
                _descriptorHeap.CPUDescriptorHandleForHeapStart + (_frameIndex * _descriptorSize));
        }

        internal void Present()
        {
            DeviceSwapChain.Present(1, PresentFlags.None);

            // Schedule a signal so that we know when the just-presented frame
            // has finished rendering. This will be checked in GetNextRenderTarget.
            var currentFenceValue = _fenceValues[_frameIndex];
            _graphicsDevice.CommandQueue.DeviceCommandQueue.Signal(_fence, currentFenceValue);

            // Advance the frame index.
            _frameIndex = DeviceSwapChain.CurrentBackBufferIndex;

            // Check to see if the next frame is ready to start.
            if (_fence.CompletedValue < _fenceValues[_frameIndex])
            {
                _fence.SetEventOnCompletion(
                    _fenceValues[_frameIndex],
                    _fenceEvent.GetSafeWaitHandle().DangerousGetHandle());

                _fenceEvent.WaitOne();
            }

            // Set the fence value for the next frame.
            _fenceValues[_frameIndex] = currentFenceValue + 1;

            _graphicsDevice.FinishFrame();
        }
    }
}
