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
        private const int FrameCount = 2;
        private const Format BackBufferFormat = Format.B8G8R8A8_UNorm;

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

        private RawViewportF _viewport;
        private RawRectangle _scissorRect;

        private RootSignature _rootSignature;
        private PipelineState _pipelineState;

        private DescriptorHeap _constantBufferViewHeap;
        private Resource _constantBuffer;
        private ConstantBuffer _constantBufferData;
        private IntPtr _constantBufferPointer;

        private D3D12Mesh _mesh;

        protected override void OnElementChanged(ElementChangedEventArgs<W3dView> e)
        {
            base.OnElementChanged(e);

            if (Control == null && e.NewElement != null)
            {
                _swapChainPanel = new SwapChainPanel();
                _swapChainPanel.SizeChanged += OnSizeChanged;

                CreateDeviceResources();

                CreateAssets();

                CreateSizeDependentResources();

                _commandList = _device.CreateCommandList(CommandListType.Direct, _commandAllocators[_frameIndex], null);
                _commandList.Close();

                CompositionTarget.Rendering += OnRendering;

                e.NewElement.SizeChanged += OnElementSizeChanged;
            }
        }

        private void OnElementSizeChanged(object sender, EventArgs e)
        {
            CreateSizeDependentResources();
        }

        private void CreateDeviceResources()
        {
#if DEBUG
            DebugInterface.Get().EnableDebugLayer();
#endif

            _device = new Device(null, SharpDX.Direct3D.FeatureLevel.Level_11_0);

            _commandQueue = _device.CreateCommandQueue(new CommandQueueDescription(CommandListType.Direct));

            var rtvHeapDesc = new DescriptorHeapDescription()
            {
                DescriptorCount = FrameCount,
                Flags = DescriptorHeapFlags.None,
                Type = DescriptorHeapType.RenderTargetView
            };

            _renderTargetViewHeap = _device.CreateDescriptorHeap(rtvHeapDesc);

            _rtvDescriptorSize = _device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);

            var cbvHeapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = 1,
                Flags = DescriptorHeapFlags.ShaderVisible,
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView
            };

            _constantBufferViewHeap = _device.CreateDescriptorHeap(cbvHeapDesc);

            for (var i = 0; i < FrameCount; i++)
            {
                _commandAllocators[i] = _device.CreateCommandAllocator(CommandListType.Direct);
            }

            _fence = _device.CreateFence(0, FenceFlags.None);
            _fenceValues[_frameIndex]++;

            _fenceEvent = new AutoResetEvent(false);

            _mesh = new D3D12Mesh(Element.W3dFile.Meshes[0]);

            _mesh.Initialize(_device);
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

            _viewport = new RawViewportF
            {
                Width = _outputSize.Width,
                Height = _outputSize.Height,
                MinDepth = 0,
                MaxDepth = 1
            };

            _scissorRect = new RawRectangle(0, 0, _outputSize.Width, _outputSize.Height);
        }

        private void CreateAssets()
        {
            var rootSignatureDesc = new RootSignatureDescription(
                RootSignatureFlags.AllowInputAssemblerInputLayout,
                new[]
                {
                    new RootParameter(ShaderVisibility.Vertex,
                        new DescriptorRange
                        {
                            RangeType = DescriptorRangeType.ConstantBufferView,
                            BaseShaderRegister = 0,
                            OffsetInDescriptorsFromTableStart = int.MinValue,
                            DescriptorCount = 1
                        })
                });
            _rootSignature = _device.CreateRootSignature(rootSignatureDesc.Serialize());

            // Create the pipeline state, which includes compiling and loading shaders.

#if DEBUG
            var vertexShader = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("shaders.hlsl", "VSMain", "vs_5_0", SharpDX.D3DCompiler.ShaderFlags.Debug));
#else
            var vertexShader = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("shaders.hlsl", "VSMain", "vs_5_0"));
#endif

#if DEBUG
            var pixelShader = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("shaders.hlsl", "PSMain", "ps_5_0", SharpDX.D3DCompiler.ShaderFlags.Debug));
#else
            var pixelShader = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("shaders.hlsl", "PSMain", "ps_5_0"));
#endif

            // Define the vertex input layout.
            var inputElementDescs = new[]
            {
                    new InputElement("POSITION",0,Format.R32G32B32_Float,0,0)
            };

            // Describe and create the graphics pipeline state object (PSO).
            var psoDesc = new GraphicsPipelineStateDescription()
            {
                InputLayout = new InputLayoutDescription(inputElementDescs),
                RootSignature = _rootSignature,
                VertexShader = vertexShader,
                PixelShader = pixelShader,
                RasterizerState = RasterizerStateDescription.Default(),
                BlendState = BlendStateDescription.Default(),
                DepthStencilFormat = SharpDX.DXGI.Format.D32_Float,
                DepthStencilState = new DepthStencilStateDescription() { IsDepthEnabled = false, IsStencilEnabled = false },
                SampleMask = int.MaxValue,
                PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
                RenderTargetCount = 1,
                Flags = PipelineStateFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                StreamOutput = new StreamOutputDescription()
            };
            psoDesc.RenderTargetFormats[0] = BackBufferFormat;

            _pipelineState = _device.CreateGraphicsPipelineState(psoDesc);

            _constantBuffer = _device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(1024 * 64),
                ResourceStates.GenericRead);

            var cbvDesc = new ConstantBufferViewDescription
            {
                BufferLocation = _constantBuffer.GPUVirtualAddress,
                SizeInBytes = (Utilities.SizeOf<ConstantBuffer>() + 255) & ~255
            };
            _device.CreateConstantBufferView(cbvDesc, _constantBufferViewHeap.CPUDescriptorHandleForHeapStart);

            // Initialize and map the constant buffers. We don't unmap this until the
            // app closes. Keeping things mapped for the lifetime of the resource is okay.
            _constantBufferPointer = _constantBuffer.Map(0);
            Utilities.Write(_constantBufferPointer, ref _constantBufferData);
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
            Update();

            PopulateCommandList();

            _commandQueue.ExecuteCommandList(_commandList);

            _swapChain.Present(1, PresentFlags.None);

            MoveToNextFrame();
        }

        private void Update()
        {
            var world = SharpDX.Matrix.Identity;
            var view = SharpDX.Matrix.LookAtLH(
                new Vector3(0, 1, 15),
                Vector3.Zero,
                Vector3.Up);
            var projection = SharpDX.Matrix.PerspectiveFovLH(
                MathUtil.DegreesToRadians(90),
                (float) (Control.ActualWidth / Control.ActualHeight),
                0.1f,
                100.0f);

            var wvp = world * view * projection;

            _constantBufferData.WorldViewProjection = wvp;

            Utilities.Write(_constantBufferPointer, ref _constantBufferData);
        }

        private void PopulateCommandList()
        {
            _commandAllocators[_frameIndex].Reset();

            _commandList.Reset(_commandAllocators[_frameIndex], _pipelineState);

            _commandList.SetGraphicsRootSignature(_rootSignature);

            _commandList.SetDescriptorHeaps(_constantBufferViewHeap);
            _commandList.SetGraphicsRootDescriptorTable(0, _constantBufferViewHeap.GPUDescriptorHandleForHeapStart);

            _commandList.SetViewports(_viewport);
            _commandList.SetScissorRectangles(_scissorRect);

            _commandList.ResourceBarrierTransition(_renderTargets[_frameIndex], ResourceStates.Present, ResourceStates.RenderTarget);

            var rtvHandle = _renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            rtvHandle += _frameIndex * _rtvDescriptorSize;

            _commandList.SetRenderTargets(rtvHandle, null);

            _commandList.ClearRenderTargetView(rtvHandle, new RawColor4(1, 0, 0, 1));

            _mesh.Draw(_commandList);

            _commandList.ResourceBarrierTransition(_renderTargets[_frameIndex], ResourceStates.RenderTarget, ResourceStates.Present);

            _commandList.Close();
        }

        protected override void Dispose(bool disposing)
        {
            CompositionTarget.Rendering -= OnRendering;

            base.Dispose(disposing);
        }

        private struct ConstantBuffer
        {
            public RawMatrix WorldViewProjection;
        }
    }
}