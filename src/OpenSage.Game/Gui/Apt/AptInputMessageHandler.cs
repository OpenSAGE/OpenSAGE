using OpenSage.Input;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    public sealed class AptInputMessageHandler : InputMessageHandler
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
                case InputMessageType.KeyDown:
                    break;
                case InputMessageType.KeyUp:
                    break;
                case InputMessageType.MouseLeftButtonDown:
                    if (_windowManager.HandleInput(message.Value.MousePosition, true))
                    {
                        return InputMessageResult.Handled;
                    }
                    break;
                case InputMessageType.MouseLeftButtonUp:
                    if (_windowManager.HandleInput(message.Value.MousePosition, false))
                    {
                        return InputMessageResult.Handled;
                    }
                    break;
                case InputMessageType.MouseMiddleButtonDown:
                    break;
                case InputMessageType.MouseMiddleButtonUp:
                    break;
                case InputMessageType.MouseRightButtonDown:
                    break;
                case InputMessageType.MouseRightButtonUp:
                    break;
                case InputMessageType.MouseMove:
                    if (_windowManager.HandleInput(message.Value.MousePosition, false))
                    {
                        return InputMessageResult.Handled;
                    }
                    break;
                case InputMessageType.MouseWheel:
                    break;
            }

            return InputMessageResult.NotHandled;
        }

    }
}
