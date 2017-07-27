using Metal;
using OpenZH.Graphics.Platforms.Metal;

namespace OpenZH.Graphics
{
    partial class PipelineState
    {
        public IMTLRenderPipelineState DeviceRenderPipelineState { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, PipelineStateDescription description)
        {
            var deviceDescriptor = new MTLRenderPipelineDescriptor
            {
                DepthAttachmentPixelFormat = MTLPixelFormat.Depth32Float,
                FragmentFunction = description.PixelShader.DeviceFunction,
                VertexDescriptor = description.VertexDescriptor.DeviceVertexDescriptor,
                VertexFunction = description.VertexShader.DeviceFunction
            };

            deviceDescriptor.ColorAttachments[0].PixelFormat = description.RenderTargetFormat.ToMTLPixelFormat();

            DeviceRenderPipelineState = AddDisposable(graphicsDevice.Device.CreateRenderPipelineState(deviceDescriptor, out var error));
        }
    }
}
