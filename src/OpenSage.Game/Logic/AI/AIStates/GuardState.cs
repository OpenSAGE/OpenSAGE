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

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            var unknownBool1 = true;
            reader.PersistBoolean(ref unknownBool1);

            reader.PersistObject(_stateMachine);
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

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();

            reader.PersistObjectID(ref _guardObjectId);
            reader.PersistObjectID(ref _guardObjectId2);
            reader.PersistVector3(ref _guardPosition);
            reader.PersistAsciiString(ref _guardPolygonTriggerName);
        }

        private sealed class GuardIdleState : State
        {
            private uint _unknownInt;

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.PersistUInt32(ref _unknownInt);
            }
        }

        private sealed class GuardUnknown5002State : State
        {
            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);
            }
        }

        private sealed class GuardMoveState : State
        {
            private uint _unknownInt;

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.PersistUInt32(ref _unknownInt);
            }
        }
    }
}
