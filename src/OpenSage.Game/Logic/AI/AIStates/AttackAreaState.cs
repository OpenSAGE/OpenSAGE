namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackAreaState : State
    {
        private readonly AttackAreaStateMachine _stateMachine;

        private uint _unknownInt;

        public AttackAreaState()
        {
            _stateMachine = new AttackAreaStateMachine();
        }

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            var unknownBool1 = true;
            reader.ReadBoolean(ref unknownBool1);
            if (!unknownBool1)
            {
                throw new InvalidStateException();
            }

            _stateMachine.Load(reader);

            reader.ReadUInt32(ref _unknownInt);
        }
    }

    internal sealed class AttackAreaStateMachine : StateMachineBase
    {
        public AttackAreaStateMachine()
        {
            AddState(0, new IdleState());
            AddState(10, new AttackState());
        }

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }
}
