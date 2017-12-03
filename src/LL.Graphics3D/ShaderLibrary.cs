namespace LL.Graphics3D
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
