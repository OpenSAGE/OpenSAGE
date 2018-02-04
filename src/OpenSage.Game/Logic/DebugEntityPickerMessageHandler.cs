using System.Numerics;
using OpenSage.Input;

namespace OpenSage.Logic
{
    public sealed class DebugEntityPickerMessageHandler : InputMessageHandler
    {
        private readonly DebugEntityPickerSystem _system;

        private Vector2 _mousePosition;
        private bool _mouseWasDown;
        private bool _mouseIsDown;

        public DebugEntityPickerMessageHandler(DebugEntityPickerSystem system)
        {
            _system = system;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    _mousePosition = new Vector2(message.MouseX.Value, message.MouseY.Value);
                    break;
                case InputMessageType.MouseDown:
                    _mouseWasDown = _mouseIsDown;

                    if (!_mouseWasDown && _system.OnClickPosition(_mousePosition))
                    {
                        return InputMessageResult.Handled;
                    }
                    break;
                case InputMessageType.MouseUp:
                    _mouseWasDown = false;
                    _mouseIsDown = false;
                    break;
            }
            return InputMessageResult.NotHandled;
        }
    }
}
