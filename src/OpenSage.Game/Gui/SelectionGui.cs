using OpenSage.Mathematics;

namespace OpenSage.Gui
{
    public sealed class SelectionGui
    {
        public Rectangle SelectionRectangle { get; set; }
        public bool SelectionBoxVisible { get; set; }

        public void Draw(DrawingContext2D context)
        {
            if (SelectionBoxVisible)
            {
                context.FillRectangle(SelectionRectangle, new ColorRgbaF(1, 1, 1, 0.1f));
                context.DrawRectangle(SelectionRectangle.ToRectangleF(), ColorRgbaF.Black, 2);
            }
        }
    }
}
