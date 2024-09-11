#nullable enable

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackTeamState : State
    {
        private readonly AttackAreaStateMachine _stateMachine;

        public AttackTeamState(AIUpdateStateMachine stateMachine) : base(stateMachine)
        {
            _stateMachine = new AttackAreaStateMachine(stateMachine);
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            var unknownBool1 = true;
            reader.PersistBoolean(ref unknownBool1);
            if (!unknownBool1)
            {
                throw new InvalidStateException();
            }

            reader.PersistObject(_stateMachine);
        }
    }
}
