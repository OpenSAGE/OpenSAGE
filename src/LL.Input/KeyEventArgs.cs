using System;

namespace LL.Input
{
    public sealed class KeyEventArgs : EventArgs
    {
        public Key Key { get; }

        public KeyEventArgs(Key key)
        {
            Key = key;
        }
    }
}
