namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackTeamState : State
    {
        private readonly AttackAreaStateMachine _stateMachine;

        public AttackTeamState()
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
        }
    }
}
