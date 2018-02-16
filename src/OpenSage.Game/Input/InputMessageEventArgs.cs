using System;

namespace OpenSage.Input
{
    public sealed class InputMessageEventArgs : EventArgs
    {
        public GameMessage Message { get; }

        public InputMessageEventArgs(GameMessage message)
        {
            Message = message;
        }
    }
}
