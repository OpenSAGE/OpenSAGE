namespace OpenSage.Scripting;

public sealed class ScriptExecutionContext
{
    public readonly IGame Game;

    public ScriptingSystem Scripting => Game.Scripting;

    public TimeInterval UpdateTime => Game.MapTime;

    public IScene3D Scene => Game.Scene3D;

    public ScriptExecutionContext(IGame game)
    {
        Game = game;
    }
}
