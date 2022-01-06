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

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            var unknownBool1 = true;
            reader.PersistBoolean("UnknownBool1", ref unknownBool1);
            if (!unknownBool1)
            {
                throw new InvalidStateException();
            }

            reader.PersistObject("StateMachine", _stateMachine);
            reader.PersistUInt32("UnknownInt", ref _unknownInt);
        }
    }

    internal sealed class AttackAreaStateMachine : StateMachineBase
    {
        public AttackAreaStateMachine()
        {
            AddState(0, new IdleState());
            AddState(10, new AttackState());
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();
        }
    }
}
