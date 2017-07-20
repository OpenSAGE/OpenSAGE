namespace OpenZH.Graphics
{
    public sealed partial class ShaderLibrary : GraphicsDeviceChild
    {
        public ShaderLibrary(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            PlatformConstruct(graphicsDevice);
        }
    }
}
