using System;
using System.Windows.Forms;
using LL.Graphics3D.Hosting;
using Swf = System.Windows.Forms;

namespace LL.Input
{
    public class InputView : GraphicsView, IInputView
    {
        public event EventHandler<KeyEventArgs> InputKeyDown;
        public event EventHandler<KeyEventArgs> InputKeyUp;
        public event EventHandler InputMouseEnter;
        public event EventHandler InputMouseExit;
        public event EventHandler<MouseEventArgs> InputMouseDown;
        public event EventHandler<MouseEventArgs> InputMouseMove;
        public event EventHandler<MouseEventArgs> InputMouseUp;
        public event EventHandler<MouseEventArgs> InputMouseWheel;

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            // Make sure we get special keys (arrow keys, etc.)
            e.IsInputKey = true;

            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(Swf.KeyEventArgs e)
        {
            e.Handled = true;

            InputKeyDown?.Invoke(this, CreateKeyEventArgs(e));

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(Swf.KeyEventArgs e)
        {
            e.Handled = true;

            InputKeyUp?.Invoke(this, CreateKeyEventArgs(e));

            base.OnKeyUp(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            InputMouseEnter?.Invoke(this, EventArgs.Empty);

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            InputMouseExit?.Invoke(this, EventArgs.Empty);

            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(Swf.MouseEventArgs e)
        {
            Focus();

            InputMouseDown?.Invoke(this, CreateMouseEventArgs(e));

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(Swf.MouseEventArgs e)
        {
            InputMouseMove?.Invoke(this, CreateMouseEventArgs(e));

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(Swf.MouseEventArgs e)
        {
            InputMouseUp?.Invoke(this, CreateMouseEventArgs(e));

            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(Swf.MouseEventArgs e)
        {
            InputMouseWheel?.Invoke(this, CreateMouseEventArgs(e));

            base.OnMouseWheel(e);
        }

        private static KeyEventArgs CreateKeyEventArgs(Swf.KeyEventArgs e)
        {
            return new KeyEventArgs(MapKey(e.KeyCode));
        }

        private MouseEventArgs CreateMouseEventArgs(Swf.MouseEventArgs e)
        {
            var controlPosition = PointToClient(e.Location);

            return new MouseEventArgs(
                MapButton(e.Button),
                e.Clicks,
                e.Delta,
                controlPosition.X,
                controlPosition.Y);
        }

        private static MouseButton MapButton(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.None:
                    return MouseButton.None;

                case MouseButtons.Left:
                    return MouseButton.Left;

                case MouseButtons.Right:
                    return MouseButton.Right;

                case MouseButtons.Middle:
                    return MouseButton.Middle;

                case MouseButtons.XButton1:
                    return MouseButton.XButton1;
                    
                case MouseButtons.XButton2:
                    return MouseButton.XButton2;

                default:
                    throw new ArgumentOutOfRangeException(nameof(button));
            }
        }

        private static Key MapKey(Keys key)
        {
            switch (key)
            {
                case Keys.Back:
                    return Key.Backspace;
                case Keys.Tab:
                    return Key.Tab;
                case Keys.Return:
                    return Key.Enter;
                case Keys.Escape:
                    return Key.Escape;
                case Keys.Space:
                    return Key.Space;
                case Keys.Left:
                    return Key.Left;
                case Keys.Up:
                    return Key.Up;
                case Keys.Right:
                    return Key.Right;
                case Keys.Down:
                    return Key.Down;
                case Keys.Delete:
                    return Key.Delete;
                case Keys.D0:
                    return Key.D0;
                case Keys.D1:
                    return Key.D1;
                case Keys.D2:
                    return Key.D2;
                case Keys.D3:
                    return Key.D3;
                case Keys.D4:
                    return Key.D4;
                case Keys.D5:
                    return Key.D5;
                case Keys.D6:
                    return Key.D6;
                case Keys.D7:
                    return Key.D7;
                case Keys.D8:
                    return Key.D8;
                case Keys.D9:
                    return Key.D9;
                case Keys.A:
                    return Key.A;
                case Keys.B:
                    return Key.B;
                case Keys.C:
                    return Key.C;
                case Keys.D:
                    return Key.D;
                case Keys.E:
                    return Key.E;
                case Keys.F:
                    return Key.F;
                case Keys.G:
                    return Key.G;
                case Keys.H:
                    return Key.H;
                case Keys.I:
                    return Key.I;
                case Keys.J:
                    return Key.J;
                case Keys.K:
                    return Key.K;
                case Keys.L:
                    return Key.L;
                case Keys.M:
                    return Key.M;
                case Keys.N:
                    return Key.N;
                case Keys.O:
                    return Key.O;
                case Keys.P:
                    return Key.P;
                case Keys.Q:
                    return Key.Q;
                case Keys.R:
                    return Key.R;
                case Keys.S:
                    return Key.S;
                case Keys.T:
                    return Key.T;
                case Keys.U:
                    return Key.U;
                case Keys.V:
                    return Key.V;
                case Keys.W:
                    return Key.W;
                case Keys.X:
                    return Key.X;
                case Keys.Y:
                    return Key.Y;
                case Keys.Z:
                    return Key.Z;
                case Keys.NumPad0:
                    return Key.NumPad0;
                case Keys.NumPad1:
                    return Key.NumPad1;
                case Keys.NumPad2:
                    return Key.NumPad2;
                case Keys.NumPad3:
                    return Key.NumPad3;
                case Keys.NumPad4:
                    return Key.NumPad4;
                case Keys.NumPad5:
                    return Key.NumPad5;
                case Keys.NumPad6:
                    return Key.NumPad6;
                case Keys.NumPad7:
                    return Key.NumPad7;
                case Keys.NumPad8:
                    return Key.NumPad8;
                case Keys.NumPad9:
                    return Key.NumPad9;
                case Keys.Divide:
                    return Key.NumPadDivide;
                case Keys.F1:
                    return Key.F1;
                case Keys.F2:
                    return Key.F2;
                case Keys.F3:
                    return Key.F3;
                case Keys.F4:
                    return Key.F4;
                case Keys.F5:
                    return Key.F5;
                case Keys.F6:
                    return Key.F6;
                case Keys.F7:
                    return Key.F7;
                case Keys.F8:
                    return Key.F8;
                case Keys.F9:
                    return Key.F9;
                case Keys.F10:
                    return Key.F10;
                case Keys.F11:
                    return Key.F11;
                case Keys.F12:
                    return Key.F12;
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                    return Key.Ctrl;
                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    return Key.Shift;
                case Keys.Alt:
                    return Key.Alt;
                default:
                    return Key.None;
            }
        }
    }
}
