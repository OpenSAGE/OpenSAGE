using System;

namespace LLGfx.Hosting
{
    public interface IGraphicsView
    {
        event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        event EventHandler<GraphicsEventArgs> GraphicsDraw;
        event EventHandler GraphicsUninitialized;
    }
}
