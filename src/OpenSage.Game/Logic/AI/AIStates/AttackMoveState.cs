namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackMoveState : MoveTowardsState
    {
        private readonly AttackMoveStateMachine _stateMachine;

        private int _unknownInt1;
        private int _unknownInt2;

        public AttackMoveState()
        {
            _stateMachine = new AttackMoveStateMachine();
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
        public AttackMoveStateMachine()
        {
            AddState(0, new IdleState());
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Persist(reader);
        }
    }
}
