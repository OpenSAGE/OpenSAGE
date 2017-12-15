using OpenSage.LowLevel.Graphics3D.Util;
using Metal;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class PipelineState
    {
        private MTLTriangleFillMode _triangleFillMode;
        private MTLWinding _frontFacingWinding;
        private MTLCullMode _cullMode;

        private IMTLDepthStencilState _depthStencilState;

        internal IMTLRenderPipelineState DeviceRenderPipelineState { get; private set; }

        internal override string PlatformGetDebugName() => DeviceRenderPipelineState.Label;
        internal override void PlatformSetDebugName(string value) { }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, PipelineStateDescription description)
        {
            using (var descriptor = new MTLRenderPipelineDescriptor())
            {
                descriptor.DepthAttachmentPixelFormat = MTLPixelFormat.Depth32Float;

                descriptor.InputPrimitiveTopology = description.PrimitiveType.ToMTLPrimitiveTopologyClass();

                descriptor.FragmentFunction = description.PixelShader.DeviceFunction;
                descriptor.VertexDescriptor = description.VertexDescriptor?.DeviceVertexDescriptor;
                descriptor.VertexFunction = description.VertexShader.DeviceFunction;

                descriptor.ColorAttachments[0].PixelFormat = description.RenderTargetFormat.ToMTLPixelFormat();

                descriptor.ColorAttachments[0].BlendingEnabled = description.BlendState.Enabled;
                descriptor.ColorAttachments[0].RgbBlendOperation = MTLBlendOperation.Add;
                descriptor.ColorAttachments[0].AlphaBlendOperation = MTLBlendOperation.Add;
                descriptor.ColorAttachments[0].SourceRgbBlendFactor = description.BlendState.SourceBlend.ToMTLBlendFactor();
                descriptor.ColorAttachments[0].SourceAlphaBlendFactor = description.BlendState.SourceAlphaBlend.ToMTLBlendFactor();
                descriptor.ColorAttachments[0].DestinationRgbBlendFactor = description.BlendState.DestinationBlend.ToMTLBlendFactor();
                descriptor.ColorAttachments[0].DestinationAlphaBlendFactor = description.BlendState.DestinationAlphaBlend.ToMTLBlendFactor();

                DeviceRenderPipelineState = AddDisposable(graphicsDevice.Device.CreateRenderPipelineState(descriptor, out _));
            }

            // Rasterizer state.
            _triangleFillMode = description.RasterizerState.FillMode.ToMTLTriangleFillMode();
            _frontFacingWinding = description.RasterizerState.IsFrontCounterClockwise
                ? MTLWinding.CounterClockwise
                : MTLWinding.Clockwise;
            _cullMode = description.RasterizerState.CullMode.ToMTLCullMode();

            // Depth stencil state.
            _depthStencilState = AddDisposable(graphicsDevice.Device.CreateDepthStencilState(new MTLDepthStencilDescriptor
            {
                DepthCompareFunction = description.DepthStencilState.DepthComparison.ToMTLCompareFunction(),
                DepthWriteEnabled = description.DepthStencilState.IsDepthWriteEnabled
            }));
        }

        internal void Apply(IMTLRenderCommandEncoder commandEncoder)
        {
            // Rasterizer state.
            commandEncoder.SetTriangleFillMode(_triangleFillMode);
            commandEncoder.SetFrontFacingWinding(_frontFacingWinding);
            commandEncoder.SetCullMode(_cullMode);

            // Depth stencil state.
            commandEncoder.SetDepthStencilState(_depthStencilState);
        }
    }
}
