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
            reader.ReadVersion(2);

            base.Load(reader);

            reader.ReadInt32(ref _unknownInt1);
            reader.ReadInt32(ref _unknownInt2);

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
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }
}
