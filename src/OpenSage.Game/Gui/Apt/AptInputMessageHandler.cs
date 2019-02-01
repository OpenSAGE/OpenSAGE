using System;
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
            foreach (var window in _windowManager.WindowStack)
            {
                switch (message.MessageType)
                {
                    case InputMessageType.KeyDown:
                        break;
                    case InputMessageType.KeyUp:
                        break;
                    case InputMessageType.MouseLeftButtonDown:
                        break;
                    case InputMessageType.MouseLeftButtonUp:
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
                        MouseMoved?.Invoke(this, new EventArgs(),
                                        message.Value.MousePosition.X,
                                        message.Value.MousePosition.Y);
                        break;
                    case InputMessageType.MouseWheel:
                        break;
                }
            }

            return InputMessageResult.NotHandled;
        }


        // Mouse moved
        public delegate bool MouseMovedEventHandler(object sender, EventArgs e, int x, int y);
        public event MouseMovedEventHandler MouseMoved;
    }
}
