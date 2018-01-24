namespace OpenSage.LowLevel.Graphics3D
{
    public sealed partial class RenderTarget : GraphicsDeviceChild
    {
        public Texture Texture { get; }

        public RenderTarget(GraphicsDevice graphicsDevice, Texture texture)
            : base(graphicsDevice)
        {
            Texture = texture;

            PlatformConstruct(graphicsDevice, texture);
        }
    }
}
