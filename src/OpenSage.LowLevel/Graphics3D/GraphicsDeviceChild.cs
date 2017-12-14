namespace OpenSage.LowLevel.Graphics3D
{
    public abstract partial class GraphicsDeviceChild : GraphicsObject
    {
        public GraphicsDevice GraphicsDevice { get; }

        public string DebugName
        {
            get => PlatformGetDebugName();
            set => PlatformSetDebugName(value);
        }

        protected GraphicsDeviceChild(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }
    }
}
