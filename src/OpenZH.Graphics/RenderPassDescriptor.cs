namespace OpenZH.Graphics
{
    public abstract class RenderPassDescriptor
    {
        public abstract void SetRenderTargetDescriptor(RenderTarget renderTargetView, LoadAction loadAction, ColorRgba clearColor = default(ColorRgba));
    }
}
