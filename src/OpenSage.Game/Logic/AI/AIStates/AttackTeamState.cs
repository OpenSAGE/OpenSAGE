namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackTeamState : State
    {
        private readonly AttackAreaStateMachine _stateMachine;

        public AttackTeamState()
        {
            _stateMachine = new AttackAreaStateMachine();
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            var unknownBool1 = true;
            reader.PersistBoolean(ref unknownBool1);
            if (!unknownBool1)
            {
                throw new InvalidStateException();
            }

            _stateMachine.Load(reader);
        }
    }
}
