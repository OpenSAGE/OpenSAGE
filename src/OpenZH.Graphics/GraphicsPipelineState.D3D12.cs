using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class GraphicsPipelineState
    {
        internal PipelineState DevicePipelineState { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, GraphicsPipelineStateDescriptor descriptor)
        {
            DevicePipelineState = AddDisposable(graphicsDevice.Device.CreateGraphicsPipelineState(descriptor.DeviceDescription));
        }
    }
}
