using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class DockState : State
    {
        private readonly DockStateMachine _stateMachine;

        private bool _unknownBool2;

        public DockState(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            _stateMachine = new DockStateMachine(gameObject, context, aiUpdate);
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
            reader.PersistBoolean(ref _unknownBool2);
        }
    }

    internal sealed class DockStateMachine : StateMachineBase
    {
        private uint _unknownInt;

        public DockStateMachine(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            AddState(0, new DockApproachDockState(gameObject, context, aiUpdate));
            AddState(1, new DockUnknown1State(gameObject, context));
            AddState(3, new DockUnknown3State(gameObject, context, aiUpdate));
            AddState(4, new DockUnknown4State(gameObject, context, aiUpdate));
            AddState(5, new DockWaitForActionState(gameObject, context));
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();

            reader.PersistUInt32(ref _unknownInt);
        }

        private sealed class DockApproachDockState : MoveTowardsState
        {
            internal DockApproachDockState(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context, aiUpdate)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(2);

                reader.BeginObject("Base");
                base.Persist(reader);
                reader.EndObject();
            }
        }

        private sealed class DockUnknown1State : State
        {
            private uint _unknownInt;

            internal DockUnknown1State(GameObject gameObject, GameContext context) : base(gameObject, context)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(2);

                reader.PersistUInt32(ref _unknownInt);
            }
        }

        private sealed class DockUnknown3State : MoveTowardsState
        {
            internal DockUnknown3State(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context, aiUpdate)
            {
            }
        }

        private sealed class DockUnknown4State : MoveTowardsState
        {
            internal DockUnknown4State(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context, aiUpdate)
            {
            }
        }

        private sealed class DockWaitForActionState : State
        {
            // Time spent in this state matches SupplyWarehouseActionDelay

            internal DockWaitForActionState(GameObject gameObject, GameContext context) : base(gameObject, context)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);
            }
        }
    }
}
