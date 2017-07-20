using System;

namespace OpenZH.Graphics.Hosting
{
    public class GraphicsEventArgs : EventArgs
    {
        public GraphicsDevice GraphicsDevice { get; }

        public GraphicsEventArgs(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }
    }
}
