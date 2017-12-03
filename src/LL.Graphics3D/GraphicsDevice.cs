namespace LL.Graphics3D
{
    public sealed partial class GraphicsDevice : GraphicsObject
    {
        public CommandQueue CommandQueue { get; }

        public ShaderLibrary ShaderLibrary { get; }

        public PixelFormat BackBufferFormat => PlatformBackBufferFormat;

        public SamplerState SamplerAnisotropicWrap { get; }
        public SamplerState SamplerLinearWrap { get; }
        public SamplerState SamplerPointWrap { get; }

        public GraphicsDevice()
        {
            PlatformConstruct();

            CommandQueue = AddDisposable(new CommandQueue(this));

            ShaderLibrary = AddDisposable(new ShaderLibrary(this));

            SamplerAnisotropicWrap = AddDisposable(new SamplerState(this, SamplerStateDescription.AnisotropicWrap) { DebugName = "AnisotropicWrap SamplerState" });
            SamplerLinearWrap = AddDisposable(new SamplerState(this, SamplerStateDescription.LinearWrap) { DebugName = "LinearWrap SamplerState" });
            SamplerPointWrap = AddDisposable(new SamplerState(this, SamplerStateDescription.PointWrap) { DebugName = "PointWrap SamplerState" });
        }
    }
}
