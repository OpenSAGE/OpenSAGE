namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackAreaState : State
    {
        private readonly AttackAreaStateMachine _stateMachine;

        public AttackAreaState()
        {
            _stateMachine = new AttackAreaStateMachine();
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool1 = reader.ReadBoolean();
            if (!unknownBool1)
            {
                throw new InvalidStateException();
            }

            _stateMachine.Load(reader);

            var unknownInt = reader.ReadUInt32();
        }
    }

    internal sealed class AttackAreaStateMachine : StateMachineBase
    {
        public AttackAreaStateMachine()
        {
            AddState(0, new IdleState());
            AddState(10, new AttackState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }
}
