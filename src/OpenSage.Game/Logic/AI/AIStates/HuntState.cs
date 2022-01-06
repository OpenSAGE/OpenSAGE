namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class HuntState : State
    {
        private readonly AttackAreaStateMachine _stateMachine;

        private uint _unknownInt;

        public HuntState()
        {
            _stateMachine = new AttackAreaStateMachine();
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            var unknownBool = true;
            reader.PersistBoolean("UnknownBool", ref unknownBool);
            if (!unknownBool)
            {
                throw new InvalidStateException();
            }

            reader.PersistObject("StateMachine", _stateMachine);
            reader.PersistUInt32("UnknownInt", ref _unknownInt);
        }
    }
}
