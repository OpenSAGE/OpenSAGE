namespace OpenSage.Input
{
    public abstract class InputMessageHandler
    {
        public abstract HandlingPriority Priority { get; }

        public abstract InputMessageResult HandleMessage(InputMessage message);
    }
}
