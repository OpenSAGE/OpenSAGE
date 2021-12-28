using System.IO;
using System.Numerics;
using OpenSage.Data.Sav;

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

            var unknownBool1 = reader.ReadBoolean();

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

            _guardObjectId = reader.ReadObjectID();

            _guardObjectId2 = reader.ReadObjectID();

            reader.ReadVector3(ref _guardPosition);

            _guardPolygonTriggerName = reader.ReadAsciiString();
        }

        private sealed class GuardIdleState : State
        {
            private uint _unknownInt;

            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                _unknownInt = reader.ReadUInt32();
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

                _unknownInt = reader.ReadUInt32();
            }
        }
    }
}
