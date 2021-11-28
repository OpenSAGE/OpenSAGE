using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class DockState : State
    {
        private readonly DockStateMachine _stateMachine;

        public DockState()
        {
            _stateMachine = new DockStateMachine();
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool1 = reader.ReadBoolean();
            if (!unknownBool1)
            {
                throw new InvalidDataException();
            }

            _stateMachine.Load(reader);

            var unknownBool2 = reader.ReadBoolean();
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
