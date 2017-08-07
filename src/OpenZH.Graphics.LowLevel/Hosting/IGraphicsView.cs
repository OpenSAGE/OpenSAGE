using System;

namespace OpenZH.Graphics.LowLevel.Hosting
{
    public interface IGraphicsView
    {
        event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        event EventHandler<GraphicsEventArgs> GraphicsDraw;

        bool RedrawsOnTimer { get; set; }

        void Draw();
    }
}
