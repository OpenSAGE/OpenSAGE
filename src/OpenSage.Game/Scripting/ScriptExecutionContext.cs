namespace OpenSage.Scripting
{
    public sealed class ScriptExecutionContext
    {
        private readonly Game _game;

        public ScriptingSystem Scripting => _game.Scripting;

        public GameTime UpdateTime => _game.UpdateTime;

        public Scene Scene => _game.Scene;

        public ScriptExecutionContext(Game game)
        {
            _game = game;
        }
    }
}
