using System;
using OpenSage.Input;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    internal sealed class AptInputMessageHandler : InputMessageHandler
    {
        private readonly AptWindowManager _windowManager;
        private readonly Game _game;

        public override HandlingPriority Priority => HandlingPriority.UIPriority;

        public struct MouseData
        {
            public bool Moved;
            public Point2D Position;
        }

        public MouseData _MouseData;

        public AptInputMessageHandler(AptWindowManager windowManager, Game game)
        {
            _game = game;
            _windowManager = windowManager;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    _MouseData.Position = message.Value.MousePosition;
                    _MouseData.Moved = true;
                    return InputMessageResult.Handled;
                case InputMessageType.MouseLeftButtonUp:
                    return InputMessageResult.NotHandled;

                case InputMessageType.KeyDown:
                case InputMessageType.KeyUp:
                case InputMessageType.MouseLeftButtonDown:
                case InputMessageType.MouseMiddleButtonDown:
                case InputMessageType.MouseMiddleButtonUp:
                case InputMessageType.MouseRightButtonDown:
                case InputMessageType.MouseRightButtonUp:
                case InputMessageType.MouseWheel:
                    //throw new NotImplementedException();
                    return InputMessageResult.NotHandled;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
