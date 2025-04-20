#nullable enable

using System;
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
    private static class TNGuardStateIds
    {
        /// <summary>
        /// Attack anything within this area till death.
        /// </summary>
        public static readonly StateId Inner = new(5000);

        /// <summary>
        /// Wait till something shows up to attack.
        /// </summary>
        public static readonly StateId Idle = new(5001);

        /// <summary>
        /// Attack anything within this area that has been aggressive, until the timer expires.
        /// </summary>
        public static readonly StateId Outer = new(5002);

        /// <summary>
        /// Restore to a position within the inner circle.
        /// </summary>
        public static readonly StateId Return = new(5003);

        /// <summary>
        /// Pick up a crate from an enemy we killed.
        /// </summary>
        public static readonly StateId GetCrate = new(5004);

        /// <summary>
        /// Attack something that attacked me (that I can attack).
        /// </summary>
        public static readonly StateId AttackAggressor = new(5005);
    }

    private ObjectId _guardObjectId;
    private Vector3 _guardPosition;

    public GuardInTunnelNetworkStateMachine(AIUpdateStateMachine parentStateMachine) : base(parentStateMachine)
    {
        ReadOnlySpan<StateConditionInfo> attackAggressors =
        [
            new StateConditionInfo(HasAttackedMeAndICanReturnFire, TNGuardStateIds.AttackAggressor)
        ];

        // Order matters: first state is the default state.
        // Original comment says that "return" is the start state, so that if
        // ordered to guard a position that isn't the unit's current position,
        // it moves to that position first.
        DefineState(TNGuardStateIds.Return, new GuardInTunnelNetworkEnterTunnelState(this), TNGuardStateIds.Idle, TNGuardStateIds.Inner, attackAggressors);
        DefineState(TNGuardStateIds.Idle, new GuardInTunnelNetworkIdleState(this), TNGuardStateIds.Inner, TNGuardStateIds.Return);
    }

    private static bool HasAttackedMeAndICanReturnFire(State state)
    {
        // TODO(Port): Implement this.
        throw new NotImplementedException();
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
