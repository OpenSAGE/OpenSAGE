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

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            var unknownBool = true;
            reader.PersistBoolean("UnknownBool", ref unknownBool);
            if (!unknownBool)
            {
                throw new InvalidStateException();
            }

            _stateMachine.Load(reader);

            reader.PersistUInt32("UnknownInt", ref _unknownInt);
        }
    }
}
