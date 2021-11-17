namespace OpenSage.Scripting
{
    public sealed class ScriptExecutionContext
    {
        public readonly Game Game;

        public ScriptingSystem Scripting => Game.Scripting;

        public TimeInterval UpdateTime => Game.MapTime;

        public Scene3D Scene => Game.Scene3D;

        public ScriptExecutionContext(Game game)
        {
            Game = game;
        }
    }
}
