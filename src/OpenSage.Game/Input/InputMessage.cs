using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Input
{
    public sealed class InputMessage
    {
        public static InputMessage CreateKeyUp(Key key, ModifierKeys modifiers)
        {
            return new InputMessage(InputMessageType.KeyUp, new InputMessageValue(key, modifiers));
        }

        public static InputMessage CreateKeyDown(Key key, ModifierKeys modifiers)
        {
            return new InputMessage(InputMessageType.KeyDown, new InputMessageValue(key, modifiers));
        }

        public static InputMessage CreateMouseButton(InputMessageType messageType, in Point2D position)
        {
            return new InputMessage(messageType, new InputMessageValue(position));
        }

        public static InputMessage CreateMouseMove(in Point2D position)
        {
            return new InputMessage(InputMessageType.MouseMove, new InputMessageValue(position));
        }

        public static InputMessage CreateMouseWheel(int value, in Point2D position)
        {
            return new InputMessage(InputMessageType.MouseWheel, new InputMessageValue(value, position));
        }

        public InputMessageType MessageType { get; }

        public readonly InputMessageValue Value;

        private InputMessage(InputMessageType messageType, in InputMessageValue value)
        {
            MessageType = messageType;
            Value = value;
        }
    }
}
