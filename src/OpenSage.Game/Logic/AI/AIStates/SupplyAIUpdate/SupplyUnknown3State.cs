#nullable enable

namespace OpenSage.Logic.AI.AIStates;

internal sealed class SupplyUnknown3State : State
{
    internal SupplyUnknown3State(SupplyAIUpdateStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);
    }
}
