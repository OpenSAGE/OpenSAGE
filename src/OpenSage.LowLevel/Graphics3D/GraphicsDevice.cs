namespace OpenSage.LowLevel.Graphics3D
{
    public sealed partial class GraphicsDevice : GraphicsObject
    {
        public CommandQueue CommandQueue { get; }

        public PixelFormat BackBufferFormat => PlatformBackBufferFormat;

        public SamplerState SamplerAnisotropicWrap { get; }
        public SamplerState SamplerLinearWrap { get; }
        public SamplerState SamplerPointWrap { get; }

        public SamplerState SamplerLinearClamp { get; }
        public SamplerState SamplerPointClamp { get; }

        public GraphicsDevice()
        {
            PlatformConstruct();

            CommandQueue = AddDisposable(new CommandQueue(this));

            SamplerAnisotropicWrap = AddDisposable(new SamplerState(this, SamplerStateDescription.AnisotropicWrap) { DebugName = "AnisotropicWrap SamplerState" });
            SamplerLinearWrap = AddDisposable(new SamplerState(this, SamplerStateDescription.LinearWrap) { DebugName = "LinearWrap SamplerState" });
            SamplerPointWrap = AddDisposable(new SamplerState(this, SamplerStateDescription.PointWrap) { DebugName = "PointWrap SamplerState" });

            SamplerLinearClamp = AddDisposable(new SamplerState(this, SamplerStateDescription.LinearClamp) { DebugName = "LinearClamp SamplerState" });
            SamplerPointClamp = AddDisposable(new SamplerState(this, SamplerStateDescription.PointClamp) { DebugName = "PointClamp SamplerState" });
        }
    }
}
