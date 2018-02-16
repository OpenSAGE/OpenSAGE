namespace OpenSage
{
    public abstract class GameMessageHandler
    {
        public abstract GameMessageResult HandleMessage(GameMessage message);
    }
}
