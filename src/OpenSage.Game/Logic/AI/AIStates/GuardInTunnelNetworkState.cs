using System.Numerics;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class GuardInTunnelNetworkState : State
    {
        private readonly GuardInTunnelNetworkStateMachine _stateMachine;

        public GuardInTunnelNetworkState()
        {
            _stateMachine = new GuardInTunnelNetworkStateMachine();
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            var unknownBool = true;
            reader.PersistBoolean("UnknownBool", ref unknownBool);
            if (!unknownBool)
            {
                throw new InvalidStateException();
            }

            _stateMachine.Load(reader);
        }
    }

    internal sealed class GuardInTunnelNetworkStateMachine : StateMachineBase
    {
        private uint _guardObjectId;
        private Vector3 _guardPosition;

        public GuardInTunnelNetworkStateMachine()
        {
            AddState(5001, new GuardInTunnelNetworkIdleState());
            AddState(5003, new GuardInTunnelNetworkEnterTunnelState());
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(2);

            base.Load(reader);

            reader.PersistObjectID("GuardObjectId", ref _guardObjectId);
            reader.PersistVector3("GuardPosition", ref _guardPosition);
        }

        private sealed class GuardInTunnelNetworkIdleState : State
        {
            private uint _unknownInt;

            internal override void Load(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.PersistUInt32("UnknownInt", ref _unknownInt);
            }
        }

        private sealed class GuardInTunnelNetworkEnterTunnelState : EnterContainerState
        {
            private uint _unknownInt;

            internal override void Load(StatePersister reader)
            {
                reader.PersistVersion(1);

                base.Load(reader);

                reader.PersistUInt32("UnknownInt", ref _unknownInt);
            }
        }
    }
}
