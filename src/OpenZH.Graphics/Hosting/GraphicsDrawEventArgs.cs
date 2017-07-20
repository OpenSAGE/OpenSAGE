namespace OpenZH.Graphics.Hosting
{
    public sealed class GraphicsDrawEventArgs : GraphicsEventArgs
    {
        public SwapChain SwapChain { get; }

        public GraphicsDrawEventArgs(GraphicsDevice graphicsDevice, SwapChain swapChain)
            : base(graphicsDevice)
        {
            SwapChain = swapChain;
        }
    }
}
