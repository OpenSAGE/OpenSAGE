namespace OpenZH.Graphics
{
    public sealed partial class SwapChain : GraphicsObject
    {
        public PixelFormat BackBufferFormat => PlatformBackBufferFormat;

        public RenderTarget GetNextRenderTarget()
        {
            return PlatformGetNextRenderTarget();
        }
    }
}
