namespace OpenZH.Graphics
{
    public sealed partial class SwapChain : GraphicsObject
    {
        public RenderTarget GetNextRenderTarget()
        {
            return PlatformGetNextRenderTarget();
        }
    }
}
