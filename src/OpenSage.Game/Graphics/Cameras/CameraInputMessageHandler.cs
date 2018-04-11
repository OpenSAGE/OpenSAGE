using System.Collections.Generic;
using OpenSage.Input;
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
                    {
                        var position = message.Value.MousePosition;
                        if (_leftMouseDown || _rightMouseDown || _middleMouseDown)
                        {
                            _deltaX += position.X - _lastX;
                            _deltaY += position.Y - _lastY;

                            _lastX = position.X;
                            _lastY = position.Y;
                        }
                        break;
                    }

                case InputMessageType.MouseLeftButtonDown:
                case InputMessageType.MouseMiddleButtonDown:
                case InputMessageType.MouseRightButtonDown:
                    {
                        var position = message.Value.MousePosition;
                        _lastX = position.X;
                        _lastY = position.Y;

                        switch (message.MessageType)
                        {
                            case InputMessageType.MouseLeftButtonDown:
                                _leftMouseDown = true;
                                break;

                            case InputMessageType.MouseMiddleButtonDown:
                                _middleMouseDown = true;
                                break;

                            case InputMessageType.MouseRightButtonDown:
                                _rightMouseDown = true;
                                break;
                        }
                        break;
                    }

                case InputMessageType.MouseLeftButtonUp:
                    _leftMouseDown = false;
                    break;

                case InputMessageType.MouseMiddleButtonUp:
                    _middleMouseDown = false;
                    break;

                case InputMessageType.MouseRightButtonUp:
                    _rightMouseDown = false;
                    break;

                case InputMessageType.MouseWheel:
                    _scrollWheelValue += message.Value.ScrollWheel;
                    break;

                case InputMessageType.KeyDown:
                    {
                        var key = message.Value.Key;
                        if (!_pressedKeys.Contains(key))
                        {
                            _pressedKeys.Add(key);
                        }
                        break;
                    }

                case InputMessageType.KeyUp:
                    {
                        var key = message.Value.Key;
                        _pressedKeys.Remove(key);
                        break;
                    }
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
