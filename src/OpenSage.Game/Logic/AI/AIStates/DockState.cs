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

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool1 = true;
            reader.ReadBoolean(ref unknownBool1);
            if (!unknownBool1)
            {
                throw new InvalidStateException();
            }

            _stateMachine.Load(reader);

            reader.ReadBoolean(ref _unknownBool2);
        }
    }

    internal sealed class DockStateMachine : StateMachineBase
    {
        public DockStateMachine()
        {
            AddState(0, new DockApproachDockState());
            AddState(1, new DockUnknown1State());
            AddState(3, new DockUnknown3State());
            AddState(4, new DockUnknown4State());
            AddState(5, new DockWaitForActionState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknownInt1 = reader.ReadUInt32();
        }

        private sealed class DockApproachDockState : MoveTowardsState
        {
            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(2);

                base.Load(reader);
            }
        }

        private sealed class DockUnknown1State : State
        {
            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(2);

                var unknownInt = reader.ReadUInt32();
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

            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);
            }
        }
    }
}
