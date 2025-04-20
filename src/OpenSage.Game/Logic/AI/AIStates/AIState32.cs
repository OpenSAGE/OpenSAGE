#nullable enable

namespace OpenSage.Logic.AI.AIStates;

internal sealed class AIState32 : FollowWaypointsState
{
    private readonly AIState32StateMachine _stateMachine;

    public AIState32(AIUpdateStateMachine stateMachine)
        : base(stateMachine, false)
    {
        _stateMachine = new AIState32StateMachine(stateMachine);
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        base.Persist(reader);

        reader.PersistObject(_stateMachine);
    }

    internal sealed class AIState32StateMachine : StateMachineBase
    {
        public AIState32StateMachine(AIUpdateStateMachine parentStateMachine) : base(parentStateMachine)
        {
            DefineState(
                AIStateIds.Idle,
                new IdleState(this),
                AIStateIds.Idle,
                AIStateIds.Idle);
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Persist(reader);
        }
    }

}
