using OpenSage.DebugOverlay;
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
                case InputMessageType.KeyUp:
                    if (message.Value.Modifier == ModifierKeys.Control && message.Value.Key == Key.D)
                    {
                        _overlay.ToggleDebugView();
                    }

                    if (message.Value.Modifier == ModifierKeys.Control && message.Value.Key == Key.G)
                    {
                        _overlay.ToggleGridPointDebugView();
                    }
                    break;
            }

            return InputMessageResult.NotHandled;
        }

    }
}

