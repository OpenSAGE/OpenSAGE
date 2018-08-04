using OpenSage.Input;

namespace OpenSage.Logic
{
    public class ActionMessageHandler : InputMessageHandler
    {
        private readonly ActionSystem _system;

        public override HandlingPriority Priority { get; }

        public ActionMessageHandler(ActionSystem system)
        {
            _system = system;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            switch (message.MessageType)
            {
                case InputMessageType.MouseRightButtonDown:
                    _system.OnRequestAction(message.Value.MousePosition);
                    break;
            }

            return InputMessageResult.NotHandled;
        }

    }
}

