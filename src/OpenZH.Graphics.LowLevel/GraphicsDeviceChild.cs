namespace OpenZH.Graphics.LowLevel
{
    public abstract class GraphicsDeviceChild : GraphicsObject
    {
        public GraphicsDevice GraphicsDevice { get; }

        protected GraphicsDeviceChild(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }
    }
}
