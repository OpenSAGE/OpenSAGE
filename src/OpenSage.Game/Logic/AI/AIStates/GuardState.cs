using System.Numerics;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class GuardState : State
    {
        private readonly GuardStateMachine _stateMachine;

        public GuardState()
        {
            _stateMachine = new GuardStateMachine();
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool1 = true;
            reader.ReadBoolean(ref unknownBool1);

            _stateMachine.Load(reader);
        }
    }

    internal sealed class GuardStateMachine : StateMachineBase
    {
        private uint _guardObjectId;
        private uint _guardObjectId2;
        private Vector3 _guardPosition;
        private string _guardPolygonTriggerName;

        public GuardStateMachine()
        {
            AddState(5001, new GuardIdleState());
            AddState(5002, new GuardUnknown5002State());
            AddState(5003, new GuardMoveState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            base.Load(reader);

            reader.ReadObjectID(ref _guardObjectId);
            reader.ReadObjectID(ref _guardObjectId2);
            reader.ReadVector3(ref _guardPosition);
            reader.ReadAsciiString(ref _guardPolygonTriggerName);
        }

        private sealed class GuardIdleState : State
        {
            private uint _unknownInt;

            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                reader.ReadUInt32(ref _unknownInt);
            }
        }

        private sealed class GuardUnknown5002State : State
        {
            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);
            }
        }

        private sealed class GuardMoveState : State
        {
            private uint _unknownInt;

            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                reader.ReadUInt32(ref _unknownInt);
            }
        }
    }
}
