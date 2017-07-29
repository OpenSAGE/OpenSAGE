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

//[assembly: ExportRenderer(typeof(W3dMeshView), typeof(W3dMeshViewRenderer))]

namespace OpenZH.DataViewer.UWP.Renderers
{
    using System.Diagnostics;
    using SharpDX.Direct3D12;
    using System.Runtime.InteropServices;
    using OpenZH.Data.W3d;

    public class W3dMeshViewRenderer : ViewRenderer<W3dMeshView, SwapChainPanel>
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
        private int _cbvDescriptorSize;
        private Resource _wvpConstantBuffer;
        private Resource _lightingConstantBuffer;
        private Resource _materialConstantBuffer;
        private WvpConstantBuffer _wvpConstantBufferData;
        private LightingConstantBuffer _lightingConstantBufferData;
        private MaterialConstantBuffer _materialConstantBufferData;
        private IntPtr _wvpConstantBufferPointer;
        private IntPtr _lightingConstantBufferPointer;
        private IntPtr _materialConstantBufferPointer;

        private D3D12Mesh _mesh;

        protected override void OnElementChanged(ElementChangedEventArgs<W3dMeshView> e)
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

            var cbvHeapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = 3,
                Flags = DescriptorHeapFlags.ShaderVisible,
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView
            };

            _constantBufferViewHeap = _device.CreateDescriptorHeap(cbvHeapDesc);

            _cbvDescriptorSize = _device.GetDescriptorHandleIncrementSize(DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);

            for (var i = 0; i < FrameCount; i++)
            {
                _commandAllocators[i] = _device.CreateCommandAllocator(CommandListType.Direct);
            }

            _fence = _device.CreateFence(0, FenceFlags.None);
            _fenceValues[_frameIndex]++;

            _fenceEvent = new AutoResetEvent(false);

            _mesh = new D3D12Mesh(Element.Mesh);

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
                        }),
                    new RootParameter(ShaderVisibility.Pixel,
                        new DescriptorRange
                        {
                            RangeType = DescriptorRangeType.ConstantBufferView,
                            BaseShaderRegister = 1,
                            OffsetInDescriptorsFromTableStart = int.MinValue,
                            DescriptorCount = 2
                        })
                });
            _rootSignature = _device.CreateRootSignature(rootSignatureDesc.Serialize());

            // Create the pipeline state, which includes compiling and loading shaders.

            ShaderBytecode compileShader(string entryPoint, string profile)
            {
#if DEBUG
                var shaderFlags = SharpDX.D3DCompiler.ShaderFlags.Debug;
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
            
            // Define the vertex input layout.
            var inputElementDescs = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 0, 1),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0, 2)
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

            var alignedWvpDataSize = (Utilities.SizeOf<WvpConstantBuffer>() + 255) & ~255; // CB size is required to be 256-byte aligned
            var alignedLightingDataSize = (Utilities.SizeOf<LightingConstantBuffer>() + 255) & ~255;
            var alignedMaterialDataSize = (Utilities.SizeOf<MaterialConstantBuffer>() + 255) & ~255;

            _wvpConstantBuffer = _device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(alignedWvpDataSize),
                ResourceStates.GenericRead);

            _lightingConstantBuffer = _device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(alignedLightingDataSize),
                ResourceStates.GenericRead);

            _materialConstantBuffer = _device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(alignedMaterialDataSize),
                ResourceStates.GenericRead);

            _device.CreateConstantBufferView(
                new ConstantBufferViewDescription
                {
                    BufferLocation = _wvpConstantBuffer.GPUVirtualAddress,
                    SizeInBytes = alignedWvpDataSize
                },
                _constantBufferViewHeap.CPUDescriptorHandleForHeapStart);

            _device.CreateConstantBufferView(
                new ConstantBufferViewDescription
                {
                    BufferLocation = _lightingConstantBuffer.GPUVirtualAddress,
                    SizeInBytes = alignedLightingDataSize
                },
                _constantBufferViewHeap.CPUDescriptorHandleForHeapStart + _cbvDescriptorSize);

            _device.CreateConstantBufferView(
                new ConstantBufferViewDescription
                {
                    BufferLocation = _materialConstantBuffer.GPUVirtualAddress,
                    SizeInBytes = alignedMaterialDataSize
                },
                _constantBufferViewHeap.CPUDescriptorHandleForHeapStart + _cbvDescriptorSize * 2);

            // Initialize and map the constant buffers. We don't unmap this until the
            // app closes. Keeping things mapped for the lifetime of the resource is okay.
            _wvpConstantBufferPointer = _wvpConstantBuffer.Map(0);
            Utilities.Write(_wvpConstantBufferPointer, ref _wvpConstantBufferData);

            _lightingConstantBufferPointer = _lightingConstantBuffer.Map(0);
            Utilities.Write(_lightingConstantBufferPointer, ref _lightingConstantBufferData);

            _materialConstantBufferPointer = _materialConstantBuffer.Map(0);
            Utilities.Write(_materialConstantBufferPointer, ref _materialConstantBufferData);
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
            var now = _stopwatch.ElapsedMilliseconds * 0.001;
            var updateTime = now - _lastUpdate;
            _lastUpdate = now;

            var world = SharpDX.Matrix.RotationY((float) _lastUpdate);

            var cameraPosition = new Vector3(0, 1, 15);

            var view = SharpDX.Matrix.LookAtLH(
                cameraPosition,
                Vector3.Zero,
                Vector3.Up);

            var projection = SharpDX.Matrix.PerspectiveFovLH(
                MathUtil.DegreesToRadians(90),
                (float) (Control.ActualWidth / Control.ActualHeight),
                0.1f,
                100.0f);

            _wvpConstantBufferData.WorldViewProjection = world * view * projection;
            _wvpConstantBufferData.World = world;
            Utilities.Write(_wvpConstantBufferPointer, ref _wvpConstantBufferData);

            _lightingConstantBufferData.CameraPosition = cameraPosition;
            _lightingConstantBufferData.AmbientLightColor = new RawVector3(0.3f, 0.3f, 0.3f);
            _lightingConstantBufferData.Light0Direction = Vector3.Normalize(new Vector3(-0.3f, -0.8f, -0.2f));
            _lightingConstantBufferData.Light0Color = new RawVector3(0.5f, 0.5f, 0.5f);
            Utilities.Write(_lightingConstantBufferPointer, ref _lightingConstantBufferData);

            _mesh.UpdateMaterial(ref _materialConstantBufferData);
            Utilities.Write(_materialConstantBufferPointer, ref _materialConstantBufferData);
        }

        private void PopulateCommandList()
        {
            _commandAllocators[_frameIndex].Reset();

            _commandList.Reset(_commandAllocators[_frameIndex], _pipelineState);

            _commandList.SetGraphicsRootSignature(_rootSignature);

            _commandList.SetDescriptorHeaps(_constantBufferViewHeap);
            _commandList.SetGraphicsRootDescriptorTable(0, _constantBufferViewHeap.GPUDescriptorHandleForHeapStart);
            _commandList.SetGraphicsRootDescriptorTable(1, _constantBufferViewHeap.GPUDescriptorHandleForHeapStart + _cbvDescriptorSize);

            _commandList.SetViewports(_viewport);
            _commandList.SetScissorRectangles(_scissorRect);

            _commandList.ResourceBarrierTransition(_renderTargets[_frameIndex], ResourceStates.Present, ResourceStates.RenderTarget);

            var rtvHandle = _renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            rtvHandle += _frameIndex * _rtvDescriptorSize;

            _commandList.SetRenderTargets(rtvHandle, null);

            _commandList.ClearRenderTargetView(rtvHandle, new RawColor4(0, 0, 0, 0));

            _mesh.Draw(_commandList);

            _commandList.ResourceBarrierTransition(_renderTargets[_frameIndex], ResourceStates.RenderTarget, ResourceStates.Present);

            _commandList.Close();
        }

        protected override void Dispose(bool disposing)
        {
            CompositionTarget.Rendering -= OnRendering;

            base.Dispose(disposing);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WvpConstantBuffer
        {
            public RawMatrix WorldViewProjection;
            public RawMatrix World;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct LightingConstantBuffer
        {
            [FieldOffset(0)]
            public RawVector3 CameraPosition;

            [FieldOffset(16)]
            public RawVector3 AmbientLightColor;

            [FieldOffset(32)]
            public RawVector3 Light0Direction;

            [FieldOffset(48)]
            public RawVector3 Light0Color;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MaterialConstantBuffer
        {
            [FieldOffset(0)]
            public RawVector3 MaterialAmbient;

            [FieldOffset(16)]
            public RawVector3 MaterialDiffuse;

            [FieldOffset(32)]
            public RawVector3 MaterialSpecular;

            [FieldOffset(48)]
            public RawVector3 MaterialEmissive;

            [FieldOffset(60)]
            public float MaterialShininess;

            [FieldOffset(64)]
            public float MaterialOpacity;
        }
    }
}