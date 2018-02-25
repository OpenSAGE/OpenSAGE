using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd.Images
{
    public abstract class ImageBase
    {
        public abstract void Draw(DrawingContext2D drawingContext, in Rectangle destinationRect);
    }
}
