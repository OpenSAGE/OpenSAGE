using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndWindowMessage
    {
        public WndWindowMessageType MessageType { get; }
        public Control Element { get; }
        public Point2D MousePosition { get; }
        public Key Key { get; }

        public WndWindowMessage(
            WndWindowMessageType messageType,
            Control element,
            in Point2D? mousePosition = null,
            in Key key = Key.Unknown)
        {
            MessageType = messageType;
            Element = element;
            MousePosition = mousePosition ?? Point2D.Zero;
            Key = key;
        }
    }
}
