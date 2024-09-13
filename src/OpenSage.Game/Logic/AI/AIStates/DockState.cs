#nullable enable

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class DockState : State
    {
        private readonly DockStateMachine _stateMachine;

        private bool _unknownBool2;

        public DockState(AIUpdateStateMachine stateMachine) : base(stateMachine)
        {
            _stateMachine = new DockStateMachine(stateMachine);
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

        public DockStateMachine(AIUpdateStateMachine parentStateMachine) : base(parentStateMachine)
        {
            AddState(0, new DockApproachDockState(this));
            AddState(1, new DockUnknown1State(this));
            AddState(3, new DockUnknown3State(this));
            AddState(4, new DockUnknown4State(this));
            AddState(5, new DockWaitForActionState(this));
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
            internal DockApproachDockState(DockStateMachine stateMachine) : base(stateMachine)
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

            internal DockUnknown1State(DockStateMachine stateMachine) : base(stateMachine)
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
            internal DockUnknown3State(DockStateMachine stateMachine) : base(stateMachine)
            {
            }
        }

        private sealed class DockUnknown4State : MoveTowardsState
        {
            internal DockUnknown4State(DockStateMachine stateMachine) : base(stateMachine)
            {
            }
        }

        private sealed class DockWaitForActionState : State
        {
            // Time spent in this state matches SupplyWarehouseActionDelay

            internal DockWaitForActionState(DockStateMachine stateMachine) : base(stateMachine)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);
            }
        }
    }
}
