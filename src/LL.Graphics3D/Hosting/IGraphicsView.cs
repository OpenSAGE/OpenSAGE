using System;

namespace LL.Graphics3D.Hosting
{
    public interface IGraphicsView
    {
        event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        event EventHandler<GraphicsEventArgs> GraphicsDraw;
        event EventHandler GraphicsUninitialized;
    }
}
