using System.Numerics;

namespace OpenSage.Logic
{
    public sealed class DebugEntityPickerMessageHandler : GameMessageHandler
    {
        private readonly DebugEntityPickerSystem _system;

        private Vector2 _mousePosition;
        private bool _mouseWasDown;
        private bool _mouseIsDown;

        public DebugEntityPickerMessageHandler(DebugEntityPickerSystem system)
        {
            _system = system;
        }

        public override GameMessageResult HandleMessage(GameMessage message)
        {
            switch (message.MessageType)
            {
                case GameMessageType.MouseMove:
                    var position = message.Arguments[0].Value.ScreenPosition;
                    _mousePosition = new Vector2(position.X, position.Y);
                    break;
                case GameMessageType.MouseLeftButtonDown:
                    _mouseWasDown = _mouseIsDown;

                    if (!_mouseWasDown && _system.OnClickPosition(_mousePosition))
                    {
                        return GameMessageResult.Handled;
                    }
                    break;
                case GameMessageType.MouseLeftButtonUp:
                    _mouseWasDown = false;
                    _mouseIsDown = false;
                    break;
            }
            return GameMessageResult.NotHandled;
        }
    }
}
