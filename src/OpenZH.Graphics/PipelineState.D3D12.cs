using OpenZH.Graphics.Platforms.Direct3D12;
using SharpDX.Direct3D12;
using D3D12 = SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class PipelineState
    {
        internal D3D12.PipelineState DevicePipelineState { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, PipelineStateDescription description)
        {
            var rasterizerState = RasterizerStateDescription.Default();
            rasterizerState.IsFrontCounterClockwise = false;

            var deviceDescription = new GraphicsPipelineStateDescription
            {
                BlendState = BlendStateDescription.Default(),
                DepthStencilFormat = SharpDX.DXGI.Format.D32_Float,
                //DepthStencilState = DepthStencilStateDescription.Default(),
                DepthStencilState = new DepthStencilStateDescription { IsDepthEnabled = false, IsStencilEnabled = false },
                Flags = PipelineStateFlags.None,
                InputLayout = description.VertexDescriptor?.DeviceInputLayoutDescription ?? new InputLayoutDescription(),
                PixelShader = description.PixelShader.DeviceBytecode,
                PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
                RasterizerState = rasterizerState,
                RenderTargetCount = 1,
                RootSignature = description.PipelineLayout.DeviceRootSignature,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                SampleMask = int.MaxValue,
                StreamOutput = new StreamOutputDescription(),
                VertexShader = description.VertexShader.DeviceBytecode
            };

            deviceDescription.RenderTargetFormats[0] = description.RenderTargetFormat.ToDxgiFormat();

            DevicePipelineState = AddDisposable(graphicsDevice.Device.CreateGraphicsPipelineState(deviceDescription));
        }
    }
}
