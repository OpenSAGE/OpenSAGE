using OpenZH.Graphics;

namespace OpenZH.Platform
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
