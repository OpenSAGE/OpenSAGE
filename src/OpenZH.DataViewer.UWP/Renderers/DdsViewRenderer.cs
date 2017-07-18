using System;
using OpenZH.DataViewer.Controls;
using SharpDX.DXGI;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Platform.UWP;

//[assembly: ExportRenderer(typeof(DdsView), typeof(OpenZH.DataViewer.UWP.Renderers.DdsViewRenderer))]

namespace OpenZH.DataViewer.UWP.Renderers
{
    using System.Diagnostics;
    using SharpDX.Direct3D12;
    using System.Runtime.InteropServices;
    using OpenZH.Data.W3d;
    using SharpDX;
    using System.Threading;
    using SharpDX.Mathematics.Interop;
    using Windows.UI.Xaml;

    public class DdsViewRenderer : ViewRenderer<DdsView, SwapChainPanel>
    {
        private const int FrameCount = 2;
        private const Format BackBufferFormat = Format.B8G8R8A8_UNorm;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private double _lastUpdate;

        private Device _device;

        private SwapChainPanel _swapChainPanel;
        private SwapChain3 _swapChain;

        private Size2 _outputSize;

        private DescriptorHeap _renderTargetViewHeap;
        private int _rtvDescriptorSize;
        private readonly Resource[] _renderTargets = new Resource[FrameCount];

        private DescriptorHeap _shaderResourceViewHeap;
        private DescriptorHeap _samplerHeap;

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

        private Resource _texture;

        protected override void OnElementChanged(ElementChangedEventArgs<DdsView> e)
        {
            base.OnElementChanged(e);

            if (Control == null && e.NewElement != null)
            {
                _swapChainPanel = new SwapChainPanel();
                _swapChainPanel.SizeChanged += OnSizeChanged;

                CreateDeviceResources();

                CreateAssets();

                CreateSizeDependentResources();

                CompositionTarget.Rendering += OnRendering;

                e.NewElement.SizeChanged += OnElementSizeChanged;

                _stopwatch.Start();
                _lastUpdate = 0;
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

            _shaderResourceViewHeap = _device.CreateDescriptorHeap(new DescriptorHeapDescription
            {
                DescriptorCount = 1,
                Flags = DescriptorHeapFlags.ShaderVisible,
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView
            });

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

            _outputSize = new Size2((int)ActualWidth, (int)ActualHeight);
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
            var sampler = new StaticSamplerDescription
            {
                Filter = Filter.MinMagMipPoint,
                AddressUVW = TextureAddressMode.Border,
                MipLODBias = 0,
                MaxAnisotropy = 0,
                ComparisonFunc = Comparison.Never,
                BorderColor = StaticBorderColor.TransparentBlack,
                MinLOD = 0.0f,
                MaxLOD = float.MaxValue,
                ShaderRegister = 0,
                RegisterSpace = 0,
                ShaderVisibility = ShaderVisibility.Pixel
            };

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
                        }),
                    new RootParameter(ShaderVisibility.Pixel,
                        new DescriptorRange
                        {
                            RangeType = DescriptorRangeType.ConstantBufferView,
                            BaseShaderRegister = 1,
                            OffsetInDescriptorsFromTableStart = int.MinValue,
                            DescriptorCount = 2
                        })
                },
                new[] { sampler });
            _rootSignature = _device.CreateRootSignature(rootSignatureDesc.Serialize());

            // Create the pipeline state, which includes compiling and loading shaders.

            ShaderBytecode compileShader(string entryPoint, string profile)
            {
#if DEBUG
                var shaderFlags = SharpDX.D3DCompiler.ShaderFlags.Debug | SharpDX.D3DCompiler.ShaderFlags.SkipOptimization;
#else
                var shaderFlags = SharpDX.D3DCompiler.ShaderFlags.None;
#endif
                var compilationResult = SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile(
                    "shaders.hlsl",
                    entryPoint,
                    profile,
                    shaderFlags);
                if (compilationResult.HasErrors || compilationResult.Bytecode == null)
                    throw new Exception($"Could not compile shader: {compilationResult.Message}");
                return new ShaderBytecode(compilationResult.Bytecode);
            }

            var vertexShader = compileShader("VSMain", "vs_5_1");
            var pixelShader = compileShader("PSMain", "ps_5_1");

            // Describe and create the graphics pipeline state object (PSO).
            var psoDesc = new GraphicsPipelineStateDescription()
            {
                InputLayout = new InputLayoutDescription(),
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

            _commandList = _device.CreateCommandList(CommandListType.Direct, _commandAllocators[_frameIndex], null);
            _commandList.Close();

	        Resource textureUploadHeap;

            // Create the texture.
            {
          //      // Describe and create a Texture2D.
          //      var textureDesc = new ResourceDescription
          //      {
          //          MipLevels = 1,
          //          Format = Format.R8G8B8A8_UNorm,
          //          Width = TextureWidth,
          //          Height = TextureHeight,
          //          Flags = D3D12_RESOURCE_FLAG_NONE,
          //          DepthOrArraySize = 1,
          //          SampleDesc.Count = 1,
          //          SampleDesc.Quality = 0,
          //          Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D
          //          };

		        //ThrowIfFailed(m_device->CreateCommittedResource(
			       // &CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_DEFAULT),
			       // D3D12_HEAP_FLAG_NONE,
			       // &textureDesc,
			       // D3D12_RESOURCE_STATE_COPY_DEST,
			       // nullptr,
			       // IID_PPV_ARGS(&m_texture)));

		        //const UINT64 uploadBufferSize = GetRequiredIntermediateSize(m_texture.Get(), 0, 1);

		        //// Create the GPU upload buffer.
		        //ThrowIfFailed(m_device->CreateCommittedResource(
			       // &CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD),
			       // D3D12_HEAP_FLAG_NONE,
			       // &CD3DX12_RESOURCE_DESC::Buffer(uploadBufferSize),
			       // D3D12_RESOURCE_STATE_GENERIC_READ,
			       // nullptr,
			       // IID_PPV_ARGS(&textureUploadHeap)));

		        //// Copy data to the intermediate upload heap and then schedule a copy 
		        //// from the upload heap to the Texture2D.
		        //std::vector<UINT8> texture = GenerateTextureData();

		        //D3D12_SUBRESOURCE_DATA textureData = {};
		        //textureData.pData = &texture[0];
		        //textureData.RowPitch = TextureWidth * TexturePixelSize;
		        //textureData.SlicePitch = textureData.RowPitch * TextureHeight;

		        //UpdateSubresources(m_commandList.Get(), m_texture.Get(), textureUploadHeap.Get(), 0, 0, 1, &textureData);
		        //m_commandList->ResourceBarrier(1, &CD3DX12_RESOURCE_BARRIER::Transition(m_texture.Get(), D3D12_RESOURCE_STATE_COPY_DEST, D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE));

		        //// Describe and create a SRV for the texture.
		        //D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
		        //srvDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
		        //srvDesc.Format = textureDesc.Format;
		        //srvDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE2D;
		        //srvDesc.Texture2D.MipLevels = 1;
		        //m_device->CreateShaderResourceView(m_texture.Get(), &srvDesc, m_srvHeap->GetCPUDescriptorHandleForHeapStart());
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
            Update();

            PopulateCommandList();

            _commandQueue.ExecuteCommandList(_commandList);

            _swapChain.Present(1, PresentFlags.None);

            MoveToNextFrame();
        }

        private void Update()
        {
            
        }

        private void PopulateCommandList()
        {
            _commandAllocators[_frameIndex].Reset();

            _commandList.Reset(_commandAllocators[_frameIndex], _pipelineState);

            _commandList.SetGraphicsRootSignature(_rootSignature);

            //_commandList.SetDescriptorHeaps(_constantBufferViewHeap);
            //_commandList.SetGraphicsRootDescriptorTable(0, _constantBufferViewHeap.GPUDescriptorHandleForHeapStart);
            //_commandList.SetGraphicsRootDescriptorTable(1, _constantBufferViewHeap.GPUDescriptorHandleForHeapStart + _cbvDescriptorSize);

            _commandList.SetViewports(_viewport);
            _commandList.SetScissorRectangles(_scissorRect);

            _commandList.ResourceBarrierTransition(_renderTargets[_frameIndex], ResourceStates.Present, ResourceStates.RenderTarget);

            var rtvHandle = _renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            rtvHandle += _frameIndex * _rtvDescriptorSize;

            _commandList.SetRenderTargets(rtvHandle, null);

            _commandList.ClearRenderTargetView(rtvHandle, new RawColor4(0, 0, 0, 0));

            //_mesh.Draw(_commandList);

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
