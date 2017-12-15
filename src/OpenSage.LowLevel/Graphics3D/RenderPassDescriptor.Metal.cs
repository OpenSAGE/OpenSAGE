using OpenSage.LowLevel.Graphics3D.Util;
using Metal;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class RenderPassDescriptor
    {
        internal MTLRenderPassDescriptor DeviceRenderPassDescriptor { get; private set; }

        private void PlatformConstruct()
        {
            DeviceRenderPassDescriptor = AddDisposable(new MTLRenderPassDescriptor());
        }

        private void PlatformSetRenderTargetDescriptor(RenderTarget renderTargetView, LoadAction loadAction, ColorRgbaF clearColor)
        {
            var colorAttachment = DeviceRenderPassDescriptor.ColorAttachments[0];

            colorAttachment.Texture = renderTargetView.DeviceTexture;
            colorAttachment.LoadAction = loadAction.ToMTLLoadAction();
            colorAttachment.ClearColor = clearColor.ToMTLClearColor();
            colorAttachment.StoreAction = MTLStoreAction.Store;
        }

        private void PlatformSetDepthStencilDescriptor(DepthStencilBuffer depthStencilBuffer)
        {
            var depthAttachment = DeviceRenderPassDescriptor.DepthAttachment;

            depthAttachment.Texture = depthStencilBuffer.DeviceTexture;
            depthAttachment.LoadAction = MTLLoadAction.Clear;
            depthAttachment.ClearDepth = depthStencilBuffer.ClearValue;
            depthAttachment.StoreAction = MTLStoreAction.Store;
        }
    }
}
