using System;
using OpenZH.Graphics;

namespace OpenZH.Platform
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
