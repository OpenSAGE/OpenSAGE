#nullable enable

namespace OpenSage.Logic.AI.AIStates;

internal sealed class SupplyUnknown4State : State
{
    internal SupplyUnknown4State(SupplyAIUpdateStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);
    }
}
