using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class GuardInTunnelNetworkState : State
    {
        private readonly GuardInTunnelNetworkStateMachine _stateMachine;

        public GuardInTunnelNetworkState(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            _stateMachine = new GuardInTunnelNetworkStateMachine(gameObject, context, aiUpdate);
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            var unknownBool = true;
            reader.PersistBoolean(ref unknownBool);
            if (!unknownBool)
            {
                throw new InvalidStateException();
            }

            reader.PersistObject(_stateMachine);
        }
    }

    internal sealed class GuardInTunnelNetworkStateMachine : StateMachineBase
    {
        private uint _guardObjectId;
        private Vector3 _guardPosition;

        public GuardInTunnelNetworkStateMachine(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            AddState(5001, new GuardInTunnelNetworkIdleState(gameObject, context));
            AddState(5003, new GuardInTunnelNetworkEnterTunnelState(gameObject, context, aiUpdate));
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(2);

            base.Persist(reader);

            reader.PersistObjectID(ref _guardObjectId);
            reader.PersistVector3(ref _guardPosition);
        }

        private sealed class GuardInTunnelNetworkIdleState : State
        {
            private uint _unknownInt;

            internal GuardInTunnelNetworkIdleState(GameObject gameObject, GameContext context) : base(gameObject, context)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.PersistUInt32(ref _unknownInt);
            }
        }

        private sealed class GuardInTunnelNetworkEnterTunnelState : EnterContainerState
        {
            private uint _unknownInt;

            internal GuardInTunnelNetworkEnterTunnelState(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context, aiUpdate)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                base.Persist(reader);

                reader.PersistUInt32(ref _unknownInt);
            }
        }
    }
}
