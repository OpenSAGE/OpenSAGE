using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndWindowMessage
    {
        public WndWindowMessageType MessageType { get; }
        public Control Element { get; }
        public Point2D MousePosition { get; }

        public WndWindowMessage(
            WndWindowMessageType messageType,
            Control element,
            in Point2D? mousePosition = null)
        {
            MessageType = messageType;
            Element = element;
            MousePosition = mousePosition ?? Point2D.Zero;
        }
    }
}
