using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using OpenZH.DataViewer.Controls;
using OpenZH.DataViewer.UWP.Renderers;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(W3dView), typeof(W3dViewRenderer))]

namespace OpenZH.DataViewer.UWP.Renderers
{
    using SharpDX.Direct3D12;

    public class W3dViewRenderer : ViewRenderer<W3dView, SwapChainPanel>
    {
        private Device _device;

        private SwapChainPanel _swapChainPanel;
        private SwapChain3 _swapChain;

        private Size2 _outputSize;

        private DescriptorHeap _renderTargetViewHeap;
        private int _rtvDescriptorSize;
        private readonly Resource[] _renderTargets = new Resource[FrameCount];

        private readonly CommandAllocator[] _commandAllocators = new CommandAllocator[FrameCount];
        private GraphicsCommandList _commandList;

        private CommandQueue _commandQueue;

        private int _frameIndex;
        private AutoResetEvent _fenceEvent;
        private Fence _fence;
        private readonly int[] _fenceValues = new int[FrameCount];

        private const int FrameCount = 2;
        private const Format BackBufferFormat = Format.B8G8R8A8_UNorm;

        protected override void OnElementChanged(ElementChangedEventArgs<W3dView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                _swapChainPanel = new SwapChainPanel();
                _swapChainPanel.SizeChanged += OnSizeChanged;

                CreateDeviceResources();

                CreateSizeDependentResources();

                _commandList = _device.CreateCommandList(CommandListType.Direct, _commandAllocators[_frameIndex], null);
                _commandList.Close();

                CompositionTarget.Rendering += OnRendering;
            }
        }

        private void CreateDeviceResources()
        {
#if DEBUG
            DebugInterface.Get().EnableDebugLayer();
#endif

            _device = new Device(null, SharpDX.Direct3D.FeatureLevel.Level_12_0);

            _commandQueue = _device.CreateCommandQueue(new CommandQueueDescription(CommandListType.Direct));

            var rtvHeapDesc = new DescriptorHeapDescription()
            {
                DescriptorCount = FrameCount,
                Flags = DescriptorHeapFlags.None,
                Type = DescriptorHeapType.RenderTargetView
            };

            _renderTargetViewHeap = _device.CreateDescriptorHeap(rtvHeapDesc);

            _rtvDescriptorSize = _device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);

            for (var i = 0; i < FrameCount; i++)
            {
                _commandAllocators[i] = _device.CreateCommandAllocator(CommandListType.Direct);
            }

            _fence = _device.CreateFence(0, FenceFlags.None);
            _fenceValues[_frameIndex]++;

            _fenceEvent = new AutoResetEvent(false);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CreateSizeDependentResources();
        }

        private void CreateSizeDependentResources()
        {
            // Wait until all previous GPU work is complete.
            WaitForGpu();

            // Clear the previous window size specific content and updated the tracked fence values.
            for (var i = 0; i < FrameCount; i++)
            {
                _renderTargets[i]?.Dispose();
                _renderTargets[i] = null;

                _fenceValues[i] = _fenceValues[_frameIndex];
            }

            _outputSize = new Size2((int) ActualWidth, (int) ActualHeight);
            _outputSize.Width = Math.Max(_outputSize.Width, 1);
            _outputSize.Height = Math.Max(_outputSize.Height, 1);

            if (_swapChain != null)
            {
                // Swap chain already exists. Resize it.
                _swapChain.ResizeBuffers(FrameCount, _outputSize.Width, _outputSize.Height, BackBufferFormat, SwapChainFlags.None);
            }
            else
            {
                var swapChainDescription = new SwapChainDescription1()
                {
                    Width = _outputSize.Width,
                    Height = _outputSize.Height,
                    Format = BackBufferFormat,
                    Stereo = false,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = Usage.RenderTargetOutput,
                    Scaling = Scaling.Stretch,
                    BufferCount = FrameCount,
                    SwapEffect = SwapEffect.FlipSequential,
                    Flags = SwapChainFlags.None
                };

#if DEBUG
                const bool debug = true;
#else
                const bool debug = false;
#endif
                using (var dxgiFactory = new Factory2(debug))
                {
                    using (var tempSwapChain = new SwapChain1(dxgiFactory, _commandQueue, ref swapChainDescription))
                    {
                        _swapChain = tempSwapChain.QueryInterface<SwapChain3>();
                    }

                    var nativePanel = ComObject.As<ISwapChainPanelNative>(_swapChainPanel);
                    nativePanel.SwapChain = _swapChain;

                    SetNativeControl(_swapChainPanel);
                }
            }

            _frameIndex = _swapChain.CurrentBackBufferIndex;

            var rtvHandle = _renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            for (var n = 0; n < FrameCount; n++)
            {
                _renderTargets[n] = _swapChain.GetBackBuffer<Resource>(n);
                _device.CreateRenderTargetView(_renderTargets[n], null, rtvHandle);
                rtvHandle += _rtvDescriptorSize;
            }
        }

        private void WaitForGpu()
        {
            // Schedule a Signal command in the queue.
            _commandQueue.Signal(_fence, _fenceValues[_frameIndex]);

            // Wait until the fence has been crossed.
            _fence.SetEventOnCompletion(_fenceValues[_frameIndex], _fenceEvent.GetSafeWaitHandle().DangerousGetHandle());
            _fenceEvent.WaitOne();

            // Increment the fence value for the current frame.
            _fenceValues[_frameIndex]++;
        }

        private void MoveToNextFrame()
        {
            var currentFenceValue = _fenceValues[_frameIndex];
            _commandQueue.Signal(_fence, currentFenceValue);

            // Advance the frame index.
            _frameIndex = _swapChain.CurrentBackBufferIndex;
            
            // Check to see if the next frame is ready to start.
            if (_fence.CompletedValue < _fenceValues[_frameIndex])
            {
                _fence.SetEventOnCompletion(_fenceValues[_frameIndex], _fenceEvent.GetSafeWaitHandle().DangerousGetHandle());
                _fenceEvent.WaitOne();
            }

            // Set the fence value for the next frame.
            _fenceValues[_frameIndex] = currentFenceValue + 1;
        }

        private void OnRendering(object sender, object e)
        {
            PopulateCommandList();

            _commandQueue.ExecuteCommandList(_commandList);

            _swapChain.Present(1, PresentFlags.None);

            MoveToNextFrame();
        }

        private void PopulateCommandList()
        {
            _commandAllocators[_frameIndex].Reset();

            _commandList.Reset(_commandAllocators[_frameIndex], null);

            _commandList.ResourceBarrierTransition(_renderTargets[_frameIndex], ResourceStates.Present, ResourceStates.RenderTarget);

            var rtvHandle = _renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            rtvHandle += _frameIndex * _rtvDescriptorSize;

            _commandList.ClearRenderTargetView(rtvHandle, new RawColor4(1, 0, 0, 1));

            _commandList.ResourceBarrierTransition(_renderTargets[_frameIndex], ResourceStates.RenderTarget, ResourceStates.Present);

            _commandList.Close();
        }

        protected override void Dispose(bool disposing)
        {
            CompositionTarget.Rendering -= OnRendering;

            base.Dispose(disposing);
        }
    }
}