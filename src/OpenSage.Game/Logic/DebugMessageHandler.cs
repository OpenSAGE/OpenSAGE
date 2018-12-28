using OpenSage.Input;
using Veldrid;

namespace OpenSage.Logic
{
    public class DebugMessageHandler : InputMessageHandler
    {
        private readonly DebugOverlay.DebugOverlay _overlay;

        public override HandlingPriority Priority => HandlingPriority.UIPriority;

        public DebugMessageHandler(DebugOverlay.DebugOverlay overlay)
        {
            _overlay = overlay;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    _overlay.MousePosition = message.Value.MousePosition;
                    break;
                case InputMessageType.KeyDown:
                    switch (message.Value.Key)
                    {
                        case Key.F2:
                            _overlay.Toggle();
                            return InputMessageResult.Handled;
                        case Key.F3:
                            _overlay.ToggleColliders();
                            return InputMessageResult.Handled;
                    }
                    break;
            }

            return InputMessageResult.NotHandled;
        }

    }
}

