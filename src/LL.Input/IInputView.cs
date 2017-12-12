using System;

namespace LL.Input
{
    public interface IInputView
    {
        event EventHandler<KeyEventArgs> InputKeyDown;
        event EventHandler<KeyEventArgs> InputKeyUp;

        event EventHandler InputMouseEnter;
        event EventHandler InputMouseExit;

        event EventHandler<MouseEventArgs> InputMouseDown;
        event EventHandler<MouseEventArgs> InputMouseMove;
        event EventHandler<MouseEventArgs> InputMouseUp;

        event EventHandler<MouseEventArgs> InputMouseWheel;
    }
}
