using System.Collections.Generic;
using OpenSage.Mathematics;

namespace OpenSage.Gui
{
    public sealed class SelectionGui
    {
        public Rectangle SelectionRectangle { get; set; }
        public bool SelectionBoxVisible { get; set; }

        public List<Rectangle> DebugOverlays;

        public SelectionGui()
        {
            DebugOverlays = new List<Rectangle>();
        }

        public void Draw(DrawingContext2D context)
        {
            foreach (var overlay in DebugOverlays)
            {
                context.FillRectangle(overlay, new ColorRgbaF(1, 0, 0, 0.2f));
                context.DrawRectangle(overlay.ToRectangleF(), ColorRgbaF.Black, 2);
            }

            if (SelectionBoxVisible)
            {
                context.FillRectangle(SelectionRectangle, new ColorRgbaF(1, 1, 1, 0.1f));
                context.DrawRectangle(SelectionRectangle.ToRectangleF(), ColorRgbaF.Black, 2);
            }
        }
    }
}
