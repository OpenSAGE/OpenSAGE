namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class DockState : State
    {
        private readonly DockStateMachine _stateMachine;

        private bool _unknownBool2;

        public DockState()
        {
            _stateMachine = new DockStateMachine();
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
            reader.PersistBoolean("UnknownBool2", ref _unknownBool2);
        }
    }

    internal sealed class DockStateMachine : StateMachineBase
    {
        private uint _unknownInt;

        public DockStateMachine()
        {
            AddState(0, new DockApproachDockState());
            AddState(1, new DockUnknown1State());
            AddState(3, new DockUnknown3State());
            AddState(4, new DockUnknown4State());
            AddState(5, new DockWaitForActionState());
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();

            reader.PersistUInt32("UnknownInt", ref _unknownInt);
        }

        private sealed class DockApproachDockState : MoveTowardsState
        {
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

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(2);

                reader.PersistUInt32("UnknownInt", ref _unknownInt);
            }
        }

        private sealed class DockUnknown3State : MoveTowardsState
        {
        }

        private sealed class DockUnknown4State : MoveTowardsState
        {
        }

        private sealed class DockWaitForActionState : State
        {
            // Time spent in this state matches SupplyWarehouseActionDelay

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);
            }
        }
    }
}
