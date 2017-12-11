using System;

namespace LL.Input
{
    public sealed partial class InputMapper : IDisposable
    {
        public event EventHandler<KeyEventArgs> KeyDown;
        public event EventHandler<KeyEventArgs> KeyUp;

        public event EventHandler MouseEnter;
        public event EventHandler MouseExit;

        public event EventHandler<MouseEventArgs> MouseDown;
        public event EventHandler<MouseEventArgs> MouseMove;
        public event EventHandler<MouseEventArgs> MouseUp;

        public event EventHandler<MouseEventArgs> MouseWheel;

        public void Dispose()
        {
            PlatformDispose();
        }
    }
}
