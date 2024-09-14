#nullable enable

namespace OpenSage.Logic.AI.AIStates;

internal sealed class SupplyUnknown1State : State
{
    internal SupplyUnknown1State(SupplyAIUpdateStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {

    }
}
