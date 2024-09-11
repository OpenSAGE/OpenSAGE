#nullable enable

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackAreaState : State
    {
        private readonly AttackAreaStateMachine _stateMachine;

        private uint _unknownInt;

        public AttackAreaState(AIUpdateStateMachine stateMachine) : base(stateMachine)
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
            reader.PersistUInt32(ref _unknownInt);
        }
    }

    internal sealed class AttackAreaStateMachine : StateMachineBase
    {
        public AttackAreaStateMachine(AIUpdateStateMachine parentStateMachine) : base(parentStateMachine)
        {
            AddState(IdleState.StateId, new IdleState(this));
            AddState(10, new AttackState(this));
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
