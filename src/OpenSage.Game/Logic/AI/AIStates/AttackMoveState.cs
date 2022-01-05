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

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(2);

            base.Load(reader);

            reader.PersistInt32("UnknownInt1", ref _unknownInt1);
            reader.PersistInt32("UnknownInt2", ref _unknownInt2);

            _stateMachine.Load(reader);
        }
    }

    internal sealed class AttackMoveStateMachine : StateMachineBase
    {
        public AttackMoveStateMachine()
        {
            AddState(0, new IdleState());
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);
        }
    }
}
