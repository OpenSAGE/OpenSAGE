namespace OpenSage;

public abstract class GameSystem : DisposableBase
{
    /// <summary>
    /// Gets the current game.
    /// </summary>
    protected IGame Game { get; }

    /// <summary>
    /// Creates a new <see cref="GameSystem"/>.
    /// </summary>
    protected GameSystem(IGame game)
    {
        Game = game;

        game.GameSystems.Add(this);
    }

    internal virtual void OnSceneChanging() { }
    internal virtual void OnSceneChanged() { }

    /// <summary>
    /// Override this to perform any required setup.
    /// </summary>
    public virtual void Initialize() { }
}
