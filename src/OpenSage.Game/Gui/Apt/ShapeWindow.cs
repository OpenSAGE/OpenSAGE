using OpenSage.Data.Apt;
using OpenSage.Gui.Apt.Elements;

namespace OpenSage.Gui.Apt
{
    public sealed class ShapeWindow
    {
        public Geometry Shape { get; }

        internal ShapeWindow(Geometry shape)
        {
            Shape = shape;
        }
    }
}
