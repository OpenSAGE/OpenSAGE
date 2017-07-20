using Metal;

namespace OpenZH.Graphics
{
    partial class GraphicsPipelineState
    {
        public IMTLRenderPipelineState DeviceRenderPipelineState { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, GraphicsPipelineStateDescriptor descriptor)
        {
            DeviceRenderPipelineState = AddDisposable(graphicsDevice.Device.CreateRenderPipelineState(descriptor.DeviceDescriptor, out var error));
        }
    }
}
