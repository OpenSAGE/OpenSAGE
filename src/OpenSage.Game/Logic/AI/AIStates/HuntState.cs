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
            reader.ReadVersion(1);

            var unknownBool = true;
            reader.ReadBoolean(ref unknownBool);
            if (!unknownBool)
            {
                throw new InvalidStateException();
            }

            _stateMachine.Load(reader);

            reader.ReadUInt32(ref _unknownInt);
        }
    }
}
