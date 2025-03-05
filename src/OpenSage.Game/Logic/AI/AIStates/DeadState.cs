#nullable enable

namespace OpenSage.Logic.AI.AIStates;

internal sealed class DeadState : State
{
    internal DeadState(AIUpdateStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);
    }
}
