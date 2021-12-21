﻿using System.IO;
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

            var guardObjectId = reader.ReadObjectID();

            var guardObjectId2 = reader.ReadObjectID();

            var guardPosition = reader.ReadVector3();

            var guardPolygonTriggerName = reader.ReadAsciiString();
        }

        private sealed class GuardIdleState : State
        {
            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                var unknownInt1 = reader.ReadUInt32();
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
            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                var unknownInt1 = reader.ReadUInt32();
            }
        }
    }
}