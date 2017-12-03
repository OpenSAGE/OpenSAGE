using LL.Graphics3D.Util;
using SharpDX.Direct3D11;

namespace LL.Graphics3D
{
    partial class PipelineState
    {
        private InputLayout _inputLayout;

        private RasterizerState _rasterizerState;
        private BlendState _blendState;
        private DepthStencilState _depthStencilState;

        internal override string PlatformGetDebugName() => _inputLayout.DebugName;

        internal override void PlatformSetDebugName(string value)
        {
            _inputLayout.DebugName = value;
            _rasterizerState.DebugName = value;
            _blendState.DebugName = value;
            _depthStencilState.DebugName = value;
        }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, PipelineStateDescription description)
        {
            _inputLayout = AddDisposable(new InputLayout(
                graphicsDevice.Device, 
                description.VertexShader.DeviceBytecode, 
                description.VertexDescriptor.DeviceInputElements));

            _rasterizerState = AddDisposable(new RasterizerState(graphicsDevice.Device, description.RasterizerState.ToRasterizerStateDescription()));

            _blendState = AddDisposable(new BlendState(graphicsDevice.Device, description.BlendState.ToBlendStateDescription()));

            _depthStencilState = AddDisposable(new DepthStencilState(graphicsDevice.Device, description.DepthStencilState.ToDepthStencilStateDescription()));
        }

        internal void Apply(DeviceContext context)
        {
            context.InputAssembler.InputLayout = _inputLayout;

            context.VertexShader.SetShader(Description.VertexShader.DeviceShader, null, 0);

            context.Rasterizer.State = _rasterizerState;

            context.PixelShader.SetShader(Description.PixelShader.DeviceShader, null, 0);

            context.OutputMerger.BlendState = _blendState;
            context.OutputMerger.DepthStencilState = _depthStencilState;
        }
    }
}
