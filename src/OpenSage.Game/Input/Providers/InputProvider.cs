using System;
using System.Collections.Generic;
using LL.Input;

namespace OpenSage.Input.Providers
{
    public sealed class InputProvider : IInputProvider, IDisposable
    {
        private readonly IInputView _inputView;

        private readonly List<Key> _pressedKeys;

        private bool _mouseEntered;
        private MouseState _currentState;
        private bool _mouseEventSinceLastUpdate;

        public InputProvider(IInputView inputView)
        {
            _inputView = inputView;

            _pressedKeys = new List<Key>();

            _inputView.InputKeyDown += HandleKeyDown;
            _inputView.InputKeyUp += HandleKeyUp;

            _inputView.InputMouseEnter += OnMouseEnter;

            _inputView.InputMouseDown += HandleMouseDown;
            _inputView.InputMouseMove += HandleMouseMove;
            _inputView.InputMouseUp += HandleMouseUp;

            _inputView.InputMouseWheel += HandleMouseWheel;
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            lock (_pressedKeys)
            {
                if (!_pressedKeys.Contains(key))
                {
                    _pressedKeys.Add(key);
                }
            }
        }

        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            lock (_pressedKeys)
            {
                _pressedKeys.Remove(e.Key);
            }
        }

        private KeyboardState GetKeyboardState()
        {
            lock (_pressedKeys)
            {
                return new KeyboardState(_pressedKeys);
            }
        }

        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            _mouseEventSinceLastUpdate = true;

            UpdateMouseButton(e.Button, ButtonState.Pressed);
        }

        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            _mouseEventSinceLastUpdate = true;

            UpdateMouseButton(e.Button, ButtonState.Released);
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            _mouseEventSinceLastUpdate = true;

            _currentState.X = e.PositionX;
            _currentState.Y = e.PositionY;
        }

        private void HandleMouseWheel(object sender, MouseEventArgs e)
        {
            _mouseEventSinceLastUpdate = true;

            _currentState.ScrollWheelValue = e.WheelDelta;
        }

        private void UpdateMouseButton(LL.Input.MouseButton button, ButtonState buttonState)
        {
            switch (button)
            {
                case LL.Input.MouseButton.Left:
                    _currentState.LeftButton = buttonState;
                    break;

                case LL.Input.MouseButton.Right:
                    _currentState.RightButton = buttonState;
                    break;

                case LL.Input.MouseButton.Middle:
                    _currentState.MiddleButton = buttonState;
                    break;

                case LL.Input.MouseButton.XButton1:
                    _currentState.XButton1 = buttonState;
                    break;

                case LL.Input.MouseButton.XButton2:
                    _currentState.XButton2 = buttonState;
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private MouseState GetMouseState()
        {
            if (!_mouseEventSinceLastUpdate)
            {
                // Reset scroll wheel value if no scroll wheel events have happened since the last time 
                // GetMouseState() was called.
                _currentState = new MouseState(
                    _currentState.X, _currentState.Y, 0,
                    _currentState.LeftButton, _currentState.MiddleButton, _currentState.RightButton,
                    _currentState.XButton1, _currentState.XButton2);
            }
            _mouseEventSinceLastUpdate = false;
            return _currentState;
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            _mouseEntered = true;
        }

        public void UpdateInputState(InputState state)
        {
            if (_mouseEntered)
            {
                state.LastMouseState = GetMouseState();
                _mouseEntered = false;
            }
            state.CurrentKeyboardState = GetKeyboardState();
            state.CurrentMouseState = GetMouseState();
        }

        public void Dispose()
        {
            _inputView.InputMouseWheel -= HandleMouseWheel;

            _inputView.InputMouseUp -= HandleMouseUp;
            _inputView.InputMouseMove -= HandleMouseMove;
            _inputView.InputMouseDown -= HandleMouseDown;

            _inputView.InputMouseEnter -= OnMouseEnter;

            _inputView.InputKeyUp -= HandleKeyUp;
            _inputView.InputKeyDown -= HandleKeyDown;
        }
    }
}
