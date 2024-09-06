using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class ChinookExitMapState : State
{
    private readonly ChinookAIUpdate _aiUpdate;

    internal ChinookExitMapState(GameObject gameObject, GameContext context, ChinookAIUpdate aiUpdate) : base(gameObject, context)
    {
        _aiUpdate = aiUpdate;
    }

    public override void Persist(StatePersister reader) { }
}
