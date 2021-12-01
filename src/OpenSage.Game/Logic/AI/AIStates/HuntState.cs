namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class HuntState : State
    {
        private readonly AttackAreaStateMachine _stateMachine;

        public HuntState()
        {
            _stateMachine = new AttackAreaStateMachine();
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool = reader.ReadBoolean();
            if (!unknownBool)
            {
                throw new InvalidStateException();
            }

            _stateMachine.Load(reader);

            var unknownInt = reader.ReadUInt32();
        }
    }
}
