using System.Collections.Generic;
using Veldrid;

namespace OpenSage.Graphics.Cameras
{
    public sealed class CameraInputMessageHandler : GameMessageHandler
    {
        private readonly List<Key> _pressedKeys = new List<Key>();

        private bool _leftMouseDown;
        private bool _middleMouseDown;
        private bool _rightMouseDown;

        private int _lastX, _lastY;
        private int _deltaX, _deltaY;

        private int _scrollWheelValue;

        public override GameMessageResult HandleMessage(GameMessage message)
        {
            switch (message.MessageType)
            {
                case GameMessageType.MouseMove:
                    {
                        var position = message.Arguments[0].Value.ScreenPosition;
                        if (_leftMouseDown || _rightMouseDown)
                        {
                            _deltaX += position.X - _lastX;
                            _deltaY += position.Y - _lastY;

                            _lastX = position.X;
                            _lastY = position.Y;
                        }
                        break;
                    }

                case GameMessageType.MouseLeftButtonDown:
                case GameMessageType.MouseMiddleButtonDown:
                case GameMessageType.MouseRightButtonDown:
                    {
                        var position = message.Arguments[0].Value.ScreenPosition;
                        _lastX = position.X;
                        _lastY = position.Y;

                        switch (message.MessageType)
                        {
                            case GameMessageType.MouseLeftButtonDown:
                                _leftMouseDown = true;
                                break;

                            case GameMessageType.MouseMiddleButtonDown:
                                _middleMouseDown = true;
                                break;

                            case GameMessageType.MouseRightButtonDown:
                                _rightMouseDown = true;
                                break;
                        }
                        break;
                    }

                case GameMessageType.MouseLeftButtonUp:
                    _leftMouseDown = false;
                    break;

                case GameMessageType.MouseMiddleButtonUp:
                    _middleMouseDown = false;
                    break;

                case GameMessageType.MouseRightButtonUp:
                    _rightMouseDown = false;
                    break;

                case GameMessageType.MouseWheel:
                    _scrollWheelValue += message.Arguments[0].Value.Integer;
                    break;

                case GameMessageType.KeyDown:
                    {
                        var key = (Key) message.Arguments[0].Value.Integer;
                        if (!_pressedKeys.Contains(key))
                        {
                            _pressedKeys.Add(key);
                        }
                        break;
                    }

                case GameMessageType.KeyUp:
                    {
                        var key = (Key) message.Arguments[0].Value.Integer;
                        _pressedKeys.Remove(key);
                        break;
                    }
            }

            return GameMessageResult.Handled;
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
