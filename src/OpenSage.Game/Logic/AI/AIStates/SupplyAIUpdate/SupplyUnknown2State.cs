#nullable enable

namespace OpenSage.Logic.AI.AIStates;

internal sealed class SupplyUnknown2State : State
{
    internal SupplyUnknown2State(SupplyAIUpdateStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);
    }
}
