
using OpenSage.Input;

namespace OpenSage.Graphics.Cameras
{
    public sealed class CameraHoverMessageHandler : InputMessageHandler
    {
        private int _lastX, _lastY;

        public override HandlingPriority Priority => HandlingPriority.MoveCameraPriority;

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    {
                        _lastX = message.Value.MousePosition.X;
                        _lastY = message.Value.MousePosition.Y;
                        break;
                    }
            }
            return InputMessageResult.NotHandled;
        }

        public void ResetMouse(IPanel panel)
        {
            _lastX = panel.Frame.Width / 2;
            _lastY = panel.Frame.Height / 2;
        }

        public void UpdateInputState(ref CameraInputState state)
        {
            state.LastX = _lastX;
            state.LastY = _lastY;
        }
    }
}