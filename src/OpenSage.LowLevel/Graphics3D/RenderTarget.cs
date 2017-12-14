namespace OpenSage.LowLevel.Graphics3D
{
    public sealed partial class RenderTarget : GraphicsDeviceChild
    {
        public RenderTarget(GraphicsDevice graphicsDevice, Texture texture)
            : base(graphicsDevice)
        {
            PlatformConstruct(graphicsDevice, texture);
        }
    }
}
