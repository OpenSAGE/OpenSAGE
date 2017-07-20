using Metal;
using OpenZH.Graphics.Platforms.Metal;

namespace OpenZH.Graphics
{
    partial class RenderPassDescriptor
    {
        internal MTLRenderPassDescriptor DeviceDescriptor { get; private set; }

        private void PlatformConstruct()
        {
            DeviceDescriptor = new MTLRenderPassDescriptor();
        }

        private void PlatformSetRenderTargetDescriptor(RenderTarget renderTargetView, LoadAction loadAction, ColorRgba clearColor)
        {
            var colorAttachment = DeviceDescriptor.ColorAttachments[0];

            colorAttachment.Texture = renderTargetView.Texture;
            colorAttachment.LoadAction = loadAction.ToMTLLoadAction();
            colorAttachment.ClearColor = clearColor.ToMTLClearColor();
        }

        // TODO: Depth attachment, etc.
    }
}