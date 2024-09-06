using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class GuardState : State
    {
        private readonly GuardStateMachine _stateMachine;

        public GuardState(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            _stateMachine = new GuardStateMachine(gameObject, context, aiUpdate);
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

        public GuardStateMachine(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            AddState(5001, new GuardIdleState(gameObject, context));
            AddState(5002, new GuardUnknown5002State(gameObject, context));
            AddState(5003, new GuardMoveState(gameObject, context));
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

            internal GuardIdleState(GameObject gameObject, GameContext context) : base(gameObject, context)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.PersistUInt32(ref _unknownInt);
            }
        }

        private sealed class GuardUnknown5002State : State
        {
            internal GuardUnknown5002State(GameObject gameObject, GameContext context) : base(gameObject, context)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);
            }
        }

        private sealed class GuardMoveState : State
        {
            private uint _unknownInt;

            internal GuardMoveState(GameObject gameObject, GameContext context) : base(gameObject, context)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.PersistUInt32(ref _unknownInt);
            }
        }
    }
}
