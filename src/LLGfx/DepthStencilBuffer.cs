namespace LLGfx
{
    public sealed partial class DepthStencilBuffer : GraphicsDeviceChild
    {
        public int Width { get; }
        public int Height { get; }

        public float ClearValue { get; }

        public DepthStencilBuffer(GraphicsDevice graphicsDevice, int width, int height, float clearValue)
            : base(graphicsDevice)
        {
            Width = width;
            Height = height;

            ClearValue = clearValue;

            PlatformConstruct(graphicsDevice, width, height, clearValue);
        }
    }
}
