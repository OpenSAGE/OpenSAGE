using System;

namespace OpenZH.Graphics.Hosting
{
    public interface IGraphicsView
    {
        event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        event EventHandler<GraphicsDrawEventArgs> GraphicsDraw;

        bool RedrawsOnTimer { get; set; }

        void Draw();
    }
}
