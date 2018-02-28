using System;
using OpenSage.Gui;

namespace OpenSage.Graphics.Rendering
{
    public sealed class Rendering2DEventArgs : EventArgs
    {
        public DrawingContext2D DrawingContext { get; }

        public Rendering2DEventArgs(DrawingContext2D drawingContext)
        {
            DrawingContext = drawingContext;
        }
    }
}
