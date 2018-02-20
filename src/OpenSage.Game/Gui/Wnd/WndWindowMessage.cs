using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndWindowMessage
    {
        public WndWindowMessageType MessageType { get; }
        public WndWindow Element { get; }
        public Point2D MousePosition { get; }

        public WndWindowMessage(
            WndWindowMessageType messageType,
            WndWindow element,
            in Point2D? mousePosition = null)
        {
            MessageType = messageType;
            Element = element;
            MousePosition = mousePosition ?? Point2D.Zero;
        }
    }
}
