using Metal;

namespace OpenZH.Graphics.Metal
{
    public sealed class MetalRenderPassDescriptor : RenderPassDescriptor
    {
        public MTLRenderPassDescriptor Descriptor { get; }

        internal MetalRenderPassDescriptor()
        {
            Descriptor = new MTLRenderPassDescriptor();
        }

        public override void SetRenderTargetDescriptor(RenderTarget renderTargetView, LoadAction loadAction, ColorRgba clearColor)
        {
            var colorAttachment = Descriptor.ColorAttachments[0];

            colorAttachment.Texture = ((MetalRenderTarget) renderTargetView).Texture;
            colorAttachment.LoadAction = loadAction.ToMTLLoadAction();
            colorAttachment.ClearColor = clearColor.ToMTLClearColor();
        }

        // TODO: Depth attachment, etc.
    }
}