using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class DoNothingState : State
{
    internal DoNothingState(GameObject gameObject, GameContext context) : base(gameObject, context)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);
    }
}
