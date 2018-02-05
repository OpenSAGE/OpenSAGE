using Veldrid;

namespace OpenSage.Input
{
    public sealed class InputMessage
    {
        public InputMessageType MessageType { get; }

        public Key? Key { get; }

        public MouseButton? MouseButton { get; }
        public int? MouseX { get; }
        public int? MouseY { get; }
        public int? MouseScrollWheelDelta { get; }

        public InputMessage(InputMessageType keyboardMessageType, Key key)
        {
            MessageType = keyboardMessageType;
            Key = key;
        }

        public InputMessage(InputMessageType mouseMessageType, MouseButton? button, int x, int y, int mouseScrollWheelDelta)
        {
            MessageType = mouseMessageType;
            MouseButton = button;
            MouseX = x;
            MouseY = y;
            MouseScrollWheelDelta = mouseScrollWheelDelta;
        }
    }
}
