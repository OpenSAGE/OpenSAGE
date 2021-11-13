using System;

namespace OpenSage.Input
{
    public sealed class CallbackMessageHandler : InputMessageHandler
    {
        private readonly Func<InputMessage, InputMessageResult> _handleMessage;

        public CallbackMessageHandler(HandlingPriority priority, Func<InputMessage, InputMessageResult> handleMessage)
        {
            Priority = priority;
            _handleMessage = handleMessage;
        }

        public override HandlingPriority Priority { get; }

        public override InputMessageResult HandleMessage(InputMessage message) => _handleMessage(message);
    }
}
