#nullable enable

namespace OpenSage.Logic.AI.AIStates;

internal sealed class AIState6 : MoveTowardsState
{
    private int _unknownInt;
    private bool _unknownBool1;
    private bool _unknownBool2;

    internal AIState6(AIUpdateStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        base.Persist(reader);

        reader.PersistInt32(ref _unknownInt);
        reader.PersistBoolean(ref _unknownBool1);
        reader.PersistBoolean(ref _unknownBool2);
    }
}
