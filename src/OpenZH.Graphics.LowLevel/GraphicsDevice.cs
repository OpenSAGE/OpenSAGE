namespace OpenZH.Graphics.LowLevel
{
    public sealed partial class GraphicsDevice : GraphicsObject
    {
        public CommandQueue CommandQueue { get; }

        public GraphicsDevice()
        {
            PlatformConstruct();

            CommandQueue = new CommandQueue(this);
        }
    }
}
