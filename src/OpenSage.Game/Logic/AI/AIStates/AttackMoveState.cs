using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackMoveState : MoveTowardsState
    {
        private readonly AttackMoveStateMachine _stateMachine;

        private int _unknownInt1;
        private int _unknownInt2;

        public AttackMoveState(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context, aiUpdate)
        {
            _stateMachine = new AttackMoveStateMachine(gameObject, context, aiUpdate);
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
        public AttackMoveStateMachine(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            AddState(IdleState.StateId, new IdleState(gameObject, context));
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Persist(reader);
        }
    }
}
