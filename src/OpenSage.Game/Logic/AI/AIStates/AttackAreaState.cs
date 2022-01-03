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
            reader.PersistVersion(1);

            var unknownBool1 = true;
            reader.PersistBoolean("UnknownBool1", ref unknownBool1);
            if (!unknownBool1)
            {
                throw new InvalidStateException();
            }

            _stateMachine.Load(reader);

            reader.PersistUInt32(ref _unknownInt);
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
            reader.PersistVersion(1);

            base.Load(reader);
        }
    }
}
