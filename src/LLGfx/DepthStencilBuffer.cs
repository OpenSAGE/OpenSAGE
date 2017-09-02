namespace LLGfx
{
    public sealed partial class DepthStencilBuffer : GraphicsDeviceChild
    {
        public DepthStencilBuffer(GraphicsDevice graphicsDevice, int width, int height)
            : base(graphicsDevice)
        {
            PlatformConstruct(graphicsDevice, width, height);
        }
    }
}
