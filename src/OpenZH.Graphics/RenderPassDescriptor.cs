namespace OpenZH.Graphics
{
    public abstract class RenderPassDescriptor
    {
        public abstract void SetRenderTargetDescriptor(RenderTargetView renderTargetView, LoadAction loadAction, ColorRgba clearColor = default(ColorRgba));
    }
}
