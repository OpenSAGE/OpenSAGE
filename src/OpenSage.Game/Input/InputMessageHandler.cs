namespace OpenSage.Input
{
    public abstract class InputMessageHandler
    {
        public abstract InputMessageResult HandleMessage(InputMessage message);
    }
}
