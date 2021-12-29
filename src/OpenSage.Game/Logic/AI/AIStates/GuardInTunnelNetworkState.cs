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

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool = true;
            reader.ReadBoolean(ref unknownBool);
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

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            base.Load(reader);

            reader.ReadObjectID(ref _guardObjectId);
            reader.ReadVector3(ref _guardPosition);
        }

        private sealed class GuardInTunnelNetworkIdleState : State
        {
            private uint _unknownInt;

            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                reader.ReadUInt32(ref _unknownInt);
            }
        }

        private sealed class GuardInTunnelNetworkEnterTunnelState : EnterContainerState
        {
            private uint _unknownInt;

            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                base.Load(reader);

                reader.ReadUInt32(ref _unknownInt);
            }
        }
    }
}
