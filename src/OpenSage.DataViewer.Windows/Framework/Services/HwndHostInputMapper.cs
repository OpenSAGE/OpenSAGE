using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LL.Graphics3D.Hosting;
using OpenSage.Input;
using OpenSageKey = OpenSage.Data.Ini.Key;
using OpenSageKeyModifiers = OpenSage.Data.Ini.KeyModifiers;

namespace OpenSage.DataViewer.Framework.Services
{
    internal sealed class HwndHostInputMapper
    {
        private readonly UIElement _uiElement;
        private readonly List<Key> _pressedKeys;

        private readonly GraphicsDeviceControl _hwndHost;
        private MouseState _currentState;
        private bool _mouseEventSinceLastUpdate;

        public HwndHostInputMapper(GraphicsDeviceControl hwndHost)
        {
            _uiElement = hwndHost;
            _pressedKeys = new List<Key>();

            _uiElement.KeyDown += HandleKeyDown;
            _uiElement.KeyUp += HandleKeyUp;

            _hwndHost = hwndHost;

            _hwndHost.HwndMouseMove += HandleMouseEvent;
            _hwndHost.HwndMouseWheel += HandleMouseEvent;
            _hwndHost.HwndLButtonDown += HandleMouseEvent;
            _hwndHost.HwndLButtonUp += HandleMouseEvent;
            _hwndHost.HwndRButtonDown += HandleMouseEvent;
            _hwndHost.HwndRButtonUp += HandleMouseEvent;
            _hwndHost.HwndMButtonDown += HandleMouseEvent;
            _hwndHost.HwndMButtonUp += HandleMouseEvent;
            _hwndHost.HwndX1ButtonDown += HandleMouseEvent;
            _hwndHost.HwndX1ButtonUp += HandleMouseEvent;
            _hwndHost.HwndX2ButtonDown += HandleMouseEvent;
            _hwndHost.HwndX2ButtonUp += HandleMouseEvent;
        }

        private void HandleMouseEvent(object sender, HwndMouseEventArgs e)
        {
            _mouseEventSinceLastUpdate = true;
            var position = e.GetPosition(_hwndHost);
            _currentState = new MouseState(
                (int) position.X, (int) position.Y, e.WheelDelta,
                MapButtonState(e.LeftButton),
                MapButtonState(e.MiddleButton),
                MapButtonState(e.RightButton),
                MapButtonState(e.X1Button),
                MapButtonState(e.X2Button));
        }

        public MouseState GetMouseState()
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

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            var key = GetKey(e);
            lock (_pressedKeys)
                if (!_pressedKeys.Contains(key))
                    _pressedKeys.Add(key);
        }

        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            lock (_pressedKeys)
                _pressedKeys.Remove(GetKey(e));
        }

        private static Key GetKey(KeyEventArgs e)
        {
            return (e.SystemKey != Key.None) ? e.SystemKey : e.Key;
        }

        public KeyboardState GetKeyboardState()
        {
            lock (_pressedKeys)
                return new KeyboardState(
                    _pressedKeys.Select(MapKey).ToList(),
                    _pressedKeys.Select(MapKeyModifiers).ToList());
        }

        public void Dispose()
        {
            _uiElement.KeyDown -= HandleKeyDown;
            _uiElement.KeyUp -= HandleKeyUp;

            _hwndHost.HwndMouseMove -= HandleMouseEvent;
            _hwndHost.HwndMouseWheel -= HandleMouseEvent;
            _hwndHost.HwndLButtonDown -= HandleMouseEvent;
            _hwndHost.HwndLButtonUp -= HandleMouseEvent;
            _hwndHost.HwndRButtonDown -= HandleMouseEvent;
            _hwndHost.HwndRButtonUp -= HandleMouseEvent;
            _hwndHost.HwndMButtonDown -= HandleMouseEvent;
            _hwndHost.HwndMButtonUp -= HandleMouseEvent;
            _hwndHost.HwndX1ButtonDown -= HandleMouseEvent;
            _hwndHost.HwndX1ButtonUp -= HandleMouseEvent;
            _hwndHost.HwndX2ButtonDown -= HandleMouseEvent;
            _hwndHost.HwndX2ButtonUp -= HandleMouseEvent;
        }

        private static OpenSageKey MapKey(Key key)
        {
            switch (key)
            {
                case Key.Back:
                    return OpenSageKey.Backspace;
                case Key.Tab:
                    return OpenSageKey.Tab;
                case Key.Return:
                    return OpenSageKey.Enter;
                case Key.Escape:
                    return OpenSageKey.Escape;
                case Key.Space:
                    return OpenSageKey.Space;
                case Key.Left:
                    return OpenSageKey.Left;
                case Key.Up:
                    return OpenSageKey.Up;
                case Key.Right:
                    return OpenSageKey.Right;
                case Key.Down:
                    return OpenSageKey.Down;
                case Key.Delete:
                    return OpenSageKey.Delete;
                case Key.D0:
                    return OpenSageKey.D0;
                case Key.D1:
                    return OpenSageKey.D1;
                case Key.D2:
                    return OpenSageKey.D2;
                case Key.D3:
                    return OpenSageKey.D3;
                case Key.D4:
                    return OpenSageKey.D4;
                case Key.D5:
                    return OpenSageKey.D5;
                case Key.D6:
                    return OpenSageKey.D6;
                case Key.D7:
                    return OpenSageKey.D7;
                case Key.D8:
                    return OpenSageKey.D8;
                case Key.D9:
                    return OpenSageKey.D9;
                case Key.A:
                    return OpenSageKey.A;
                case Key.B:
                    return OpenSageKey.B;
                case Key.C:
                    return OpenSageKey.C;
                case Key.D:
                    return OpenSageKey.D;
                case Key.E:
                    return OpenSageKey.E;
                case Key.F:
                    return OpenSageKey.F;
                case Key.G:
                    return OpenSageKey.G;
                case Key.H:
                    return OpenSageKey.H;
                case Key.I:
                    return OpenSageKey.I;
                case Key.J:
                    return OpenSageKey.J;
                case Key.K:
                    return OpenSageKey.K;
                case Key.L:
                    return OpenSageKey.L;
                case Key.M:
                    return OpenSageKey.M;
                case Key.N:
                    return OpenSageKey.N;
                case Key.O:
                    return OpenSageKey.O;
                case Key.P:
                    return OpenSageKey.P;
                case Key.Q:
                    return OpenSageKey.Q;
                case Key.R:
                    return OpenSageKey.R;
                case Key.S:
                    return OpenSageKey.S;
                case Key.T:
                    return OpenSageKey.T;
                case Key.U:
                    return OpenSageKey.U;
                case Key.V:
                    return OpenSageKey.V;
                case Key.W:
                    return OpenSageKey.W;
                case Key.X:
                    return OpenSageKey.X;
                case Key.Y:
                    return OpenSageKey.Y;
                case Key.Z:
                    return OpenSageKey.Z;
                case Key.NumPad0:
                    return OpenSageKey.NumPad0;
                case Key.NumPad1:
                    return OpenSageKey.NumPad1;
                case Key.NumPad2:
                    return OpenSageKey.NumPad2;
                case Key.NumPad3:
                    return OpenSageKey.NumPad3;
                case Key.NumPad4:
                    return OpenSageKey.NumPad4;
                case Key.NumPad5:
                    return OpenSageKey.NumPad5;
                case Key.NumPad6:
                    return OpenSageKey.NumPad6;
                case Key.NumPad7:
                    return OpenSageKey.NumPad7;
                case Key.NumPad8:
                    return OpenSageKey.NumPad8;
                case Key.NumPad9:
                    return OpenSageKey.NumPad9;
                case Key.Divide:
                    return OpenSageKey.NumPadDivide;
                case Key.F1:
                    return OpenSageKey.F1;
                case Key.F2:
                    return OpenSageKey.F2;
                case Key.F3:
                    return OpenSageKey.F3;
                case Key.F4:
                    return OpenSageKey.F4;
                case Key.F5:
                    return OpenSageKey.F5;
                case Key.F6:
                    return OpenSageKey.F6;
                case Key.F7:
                    return OpenSageKey.F7;
                case Key.F8:
                    return OpenSageKey.F8;
                case Key.F9:
                    return OpenSageKey.F9;
                case Key.F10:
                    return OpenSageKey.F10;
                case Key.F11:
                    return OpenSageKey.F11;
                case Key.F12:
                    return OpenSageKey.F12;
                default:
                    return OpenSageKey.None;
            }
        }

        private static OpenSageKeyModifiers MapKeyModifiers(Key key)
        {
            switch (key)
            {
                case Key.LeftShift:
                case Key.RightShift:
                    return OpenSageKeyModifiers.Shift;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    return OpenSageKeyModifiers.Ctrl;
                case Key.LeftAlt:
                case Key.RightAlt:
                    return OpenSageKeyModifiers.Alt;
                default:
                    return OpenSageKeyModifiers.None;
            }
        }

        private static ButtonState MapButtonState(MouseButtonState mouseButtonState)
        {
            if (mouseButtonState == MouseButtonState.Pressed)
                return ButtonState.Pressed;
            return ButtonState.Released;
        }
    }
}
