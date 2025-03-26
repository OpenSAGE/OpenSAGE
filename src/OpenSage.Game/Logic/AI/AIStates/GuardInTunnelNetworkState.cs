#nullable enable

using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class GuardInTunnelNetworkState : State
{
    private readonly GuardInTunnelNetworkStateMachine _stateMachine;

    public GuardInTunnelNetworkState(AIUpdateStateMachine stateMachine) : base(stateMachine)
    {
        _stateMachine = new GuardInTunnelNetworkStateMachine(stateMachine);
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
    private ObjectId _guardObjectId;
    private Vector3 _guardPosition;

    public GuardInTunnelNetworkStateMachine(AIUpdateStateMachine parentStateMachine) : base(parentStateMachine)
    {
        AddState(5001, new GuardInTunnelNetworkIdleState(this));
        AddState(5003, new GuardInTunnelNetworkEnterTunnelState(this));
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(2);

        base.Persist(reader);

        reader.PersistObjectId(ref _guardObjectId);
        reader.PersistVector3(ref _guardPosition);
    }

    private sealed class GuardInTunnelNetworkIdleState : State
    {
        private uint _unknownInt;

        internal GuardInTunnelNetworkIdleState(GuardInTunnelNetworkStateMachine stateMachine) : base(stateMachine)
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

        internal GuardInTunnelNetworkEnterTunnelState(GuardInTunnelNetworkStateMachine stateMachine) : base(stateMachine)
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
