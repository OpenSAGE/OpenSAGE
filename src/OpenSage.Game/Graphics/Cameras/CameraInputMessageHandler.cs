using OpenSage.Input;
using System.Collections.Generic;
using Veldrid;

namespace OpenSage.Graphics.Cameras
{
    public sealed class CameraInputMessageHandler : InputMessageHandler
    {
        private readonly List<Key> _pressedKeys = new List<Key>();

        private bool _leftMouseDown;
        private bool _middleMouseDown;
        private bool _rightMouseDown;

        private int _lastX, _lastY;
        private int _deltaX, _deltaY;

        private int _scrollWheelValue;

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    if (_leftMouseDown)
                    {
                        _deltaX += message.MouseX.Value - _lastX;
                        _deltaY += message.MouseY.Value - _lastY;

                        _lastX = message.MouseX.Value;
                        _lastY = message.MouseY.Value;
                    }
                    break;

                case InputMessageType.MouseDown:
                    _lastX = message.MouseX.Value;
                    _lastY = message.MouseY.Value;

                    switch (message.MouseButton)
                    {
                        case MouseButton.Left:
                            _leftMouseDown = true;
                            break;

                        case MouseButton.Middle:
                            _middleMouseDown = true;
                            break;

                        case MouseButton.Right:
                            _rightMouseDown = true;
                            break;
                    }
                    break;

                case InputMessageType.MouseUp:
                    switch (message.MouseButton)
                    {
                        case MouseButton.Left:
                            _leftMouseDown = false;
                            break;

                        case MouseButton.Middle:
                            _middleMouseDown = false;
                            break;

                        case MouseButton.Right:
                            _rightMouseDown = false;
                            break;
                    }
                    break;

                case InputMessageType.MouseWheel:
                    _scrollWheelValue += message.MouseScrollWheelDelta.Value;
                    break;

                case InputMessageType.KeyDown:
                    if (!_pressedKeys.Contains(message.Key.Value))
                    {
                        _pressedKeys.Add(message.Key.Value);
                    }
                    break;

                case InputMessageType.KeyUp:
                    _pressedKeys.Remove(message.Key.Value);
                    break;
            }

            return InputMessageResult.Handled;
        }

        public void UpdateInputState(ref CameraInputState state)
        {
            state.LeftMouseDown = _leftMouseDown;
            state.MiddleMouseDown = _middleMouseDown;
            state.RightMouseDown = _rightMouseDown;

            state.DeltaX = _deltaX;
            state.DeltaY = _deltaY;

            state.ScrollWheelValue = _scrollWheelValue;

            state.PressedKeys = new List<Key>(_pressedKeys);

            _deltaX = _deltaY = _scrollWheelValue = 0;
        }
    }
}
