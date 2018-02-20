using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Input
{
    public sealed class InputMessage
    {
        public static InputMessage CreateKeyUp(Key value)
        {
            return new InputMessage(InputMessageType.KeyUp, new InputMessageValue { Key = value });
        }

        public static InputMessage CreateKeyDown(Key value)
        {
            return new InputMessage(InputMessageType.KeyDown, new InputMessageValue { Key = value });
        }

        public static InputMessage CreateMouseButton(InputMessageType messageType, in Point2D position)
        {
            return new InputMessage(messageType, new InputMessageValue { MousePosition = position });
        }

        public static InputMessage CreateMouseMove(in Point2D position)
        {
            return new InputMessage(InputMessageType.MouseMove, new InputMessageValue { MousePosition = position });
        }

        public static InputMessage CreateMouseWheel(int value)
        {
            return new InputMessage(InputMessageType.MouseWheel, new InputMessageValue { ScrollWheel = value });
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
