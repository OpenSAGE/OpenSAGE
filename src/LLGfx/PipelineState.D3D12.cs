using LLGfx.Util;
using SharpDX.Direct3D12;
using D3D12 = SharpDX.Direct3D12;

namespace LLGfx
{
    public enum Blend
    {
        Zero,
        One,
        SrcAlpha,
        OneMinusSrcAlpha
    }

    partial class PipelineState
    {
        internal D3D12.PipelineState DevicePipelineState { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, PipelineStateDescription description)
        {
            var rasterizerState = RasterizerStateDescription.Default();
            rasterizerState.IsFrontCounterClockwise = description.IsFrontCounterClockwise;

            rasterizerState.CullMode = description.TwoSided
                ? CullMode.None
                : CullMode.Back;

            var blendState = BlendStateDescription.Default();
            if (description.Blending.Enabled)
            {
                blendState.RenderTarget[0].IsBlendEnabled = true;
                blendState.RenderTarget[0].SourceBlend = description.Blending.SourceBlend.ToBlendOption();
                blendState.RenderTarget[0].SourceAlphaBlend = description.Blending.SourceBlend.ToBlendOption();
                blendState.RenderTarget[0].DestinationBlend = description.Blending.DestinationBlend.ToBlendOption();
                blendState.RenderTarget[0].DestinationAlphaBlend = description.Blending.DestinationBlend.ToBlendOption();
            }

            var depthStencilState = DepthStencilStateDescription.Default();
            depthStencilState.IsDepthEnabled = description.IsDepthEnabled;
            depthStencilState.DepthWriteMask = description.IsDepthWriteEnabled
                ? DepthWriteMask.All
                : DepthWriteMask.Zero;
            depthStencilState.DepthComparison = Comparison.LessEqual;

            var deviceDescription = new GraphicsPipelineStateDescription
            {
                BlendState = blendState,
                DepthStencilFormat = SharpDX.DXGI.Format.D32_Float,
                DepthStencilState = depthStencilState,
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
