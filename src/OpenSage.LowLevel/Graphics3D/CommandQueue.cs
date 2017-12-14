namespace OpenSage.LowLevel.Graphics3D
{
    public sealed partial class CommandQueue : GraphicsDeviceChild
    {
        internal CommandQueue(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            PlatformConstruct(graphicsDevice);
        }

        public CommandBuffer GetCommandBuffer()
        {
            return PlatformGetCommandBuffer();
        }
    }
}
