namespace LL.Graphics3D
{
    public sealed partial class RenderPassDescriptor
    {
        public RenderPassDescriptor()
        {
            PlatformConstruct();
        }

        public void SetRenderTargetDescriptor(RenderTarget renderTargetView, LoadAction loadAction, ColorRgbaF clearColor = default(ColorRgbaF))
        {
            PlatformSetRenderTargetDescriptor(renderTargetView, loadAction, clearColor);
        }

        public void SetDepthStencilDescriptor(DepthStencilBuffer depthStencilBuffer)
        {
            PlatformSetDepthStencilDescriptor(depthStencilBuffer);
        }
    }
}
