#nullable enable

namespace OpenSage.Logic.AI.AIStates;

internal sealed class DoNothingState : State
{
    internal DoNothingState(AIUpdateStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);
    }
}
