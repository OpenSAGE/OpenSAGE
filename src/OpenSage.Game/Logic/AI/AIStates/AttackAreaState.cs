using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackAreaState : State
    {
        private readonly AttackAreaStateMachine _stateMachine;

        private uint _unknownInt;

        public AttackAreaState(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            _stateMachine = new AttackAreaStateMachine(gameObject, context, aiUpdate);
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
        public AttackAreaStateMachine(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            AddState(IdleState.StateId, new IdleState(gameObject, context));
            AddState(10, new AttackState(gameObject, context, aiUpdate));
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
