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

            var unknownBool = reader.ReadBoolean();
            if (!unknownBool)
            {
                throw new InvalidStateException();
            }

            _stateMachine.Load(reader);
        }
    }

    internal sealed class GuardInTunnelNetworkStateMachine : StateMachineBase
    {
        public GuardInTunnelNetworkStateMachine()
        {
            AddState(5001, new GuardInTunnelNetworkIdleState());
            AddState(5003, new GuardInTunnelNetworkEnterTunnelState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            base.Load(reader);

            var guardObjectId = reader.ReadObjectID();

            var guardPosition = reader.ReadVector3();
        }

        private sealed class GuardInTunnelNetworkIdleState : State
        {
            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                var unknownInt1 = reader.ReadUInt32();
            }
        }

        private sealed class GuardInTunnelNetworkEnterTunnelState : EnterContainerState
        {
            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                base.Load(reader);

                var unknownInt1 = reader.ReadUInt32();
            }
        }
    }
}
