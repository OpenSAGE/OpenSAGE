namespace OpenSage.Input
{
    public sealed class InputSystem : GameSystem
    {
        public InputMessageBuffer MessageBuffer { get; }

        public InputSystem(Game game)
            : base(game)
        {
            MessageBuffer = AddDisposable(new InputMessageBuffer(game.Window));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            MessageBuffer.PropagateMessages();
        }
    }
}
