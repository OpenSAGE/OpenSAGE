#nullable enable

namespace OpenSage.Logic.AI.AIStates;

internal sealed class AttackMoveState : MoveTowardsState
{
    private readonly AttackMoveStateMachine _stateMachine;

    private int _unknownInt1;
    private int _unknownInt2;

    public AttackMoveState(AIUpdateStateMachine stateMachine) : base(stateMachine)
    {
        _stateMachine = new AttackMoveStateMachine(stateMachine);
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(2);

        base.Persist(reader);

        reader.PersistInt32(ref _unknownInt1);
        reader.PersistInt32(ref _unknownInt2);
        reader.PersistObject(_stateMachine);
    }
}

internal sealed class AttackMoveStateMachine : StateMachineBase
{
    public AttackMoveStateMachine(AIUpdateStateMachine parentStateMachine) : base(parentStateMachine)
    {
        DefineState(AIStateIds.Idle, new IdleState(this), AIStateIds.Idle, AIStateIds.Idle);
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        base.Persist(reader);
    }
}
