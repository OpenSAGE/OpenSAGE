namespace OpenSage.Scripting
{
    public sealed class ScriptExecutionContext
    {
        private readonly Game _game;

        public ScriptingSystem Scripting => _game.Scripting;

        public GameTime UpdateTime => _game.MapTime;

        public Scene3D Scene => _game.Scene3D;

        public ScriptExecutionContext(Game game)
        {
            _game = game;
        }
    }
}
