namespace OpenSage.Gui.Wnd
{
    internal sealed class WndWindowMessage
    {
        public WndWindowMessageType MessageType { get; }
        public WndWindow Element { get; }

        public WndWindowMessage(WndWindowMessageType messageType, WndWindow element)
        {
            MessageType = messageType;
            Element = element;
        }
    }
}
