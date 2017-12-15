namespace OpenSage.LowLevel.Graphics3D
{
    public sealed partial class RenderPassDescriptor : DisposableBase
    {
        public RenderPassDescriptor()
        {
            PlatformConstruct();
        }

        public void SetRenderTargetDescriptor(RenderTarget renderTargetView, LoadAction loadAction, ColorRgbaF clearColor = default)
        {
            PlatformSetRenderTargetDescriptor(renderTargetView, loadAction, clearColor);
        }

        public void SetDepthStencilDescriptor(DepthStencilBuffer depthStencilBuffer)
        {
            PlatformSetDepthStencilDescriptor(depthStencilBuffer);
        }
    }
}
