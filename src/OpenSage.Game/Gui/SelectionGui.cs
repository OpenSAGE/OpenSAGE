using System.Collections.Generic;
using OpenSage.Graphics.Cameras;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Gui
{
    public sealed class SelectionGui
    {
        public Rectangle SelectionRectangle { get; set; }
        public bool SelectionBoxVisible { get; set; }

        public List<Collider> SelectedObjects { get; set; }

        public SelectionGui()
        {
            SelectedObjects = new List<Collider>();
        }

        public void Draw(DrawingContext2D context, PerspectiveCamera camera)
        {
            foreach (var collider in SelectedObjects)
            {
                var rect = collider.GetBoundingRectangle(camera);
                context.FillRectangle(rect, new ColorRgbaF(1, 0, 0, 0.2f));
                context.DrawRectangle(rect.ToRectangleF(), ColorRgbaF.Black, 2);
            }

            if (SelectionBoxVisible)
            {
                context.FillRectangle(SelectionRectangle, new ColorRgbaF(1, 1, 1, 0.1f));
                context.DrawRectangle(SelectionRectangle.ToRectangleF(), ColorRgbaF.Black, 2);
            }
        }
    }
}
