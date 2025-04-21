#nullable enable

using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class AttackState : State
{
    private readonly AttackStateMachine _stateMachine;

    private bool _unknownBool;
    private Vector3 _unknownPosition;

    public AttackState(StateMachineBase stateMachine) : base(stateMachine)
    {
        _stateMachine = new AttackStateMachine(stateMachine);
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistBoolean(ref _unknownBool);
        reader.PersistVector3(ref _unknownPosition);
        reader.PersistObject(_stateMachine);
    }
}

internal static class AttackStateIds
{
    /// <summary>
    /// Chase a moving target (optionally following it).
    /// </summary>
    public static readonly StateId ChaseTarget = new(0);

    /// <summary>
    /// Approach a non-moving target.
    /// </summary>
    public static readonly StateId ApproachTarget = new(1);

    /// <summary>
    /// Rotate to face GoalObject or GoalPosition.
    /// </summary>
    public static readonly StateId AimAtTarget = new(2);

    /// <summary>
    /// Fire the machine owner's current weapon.
    /// </summary>
    public static readonly StateId FireWeapon = new(3);
}

internal sealed class AttackStateMachine : StateMachineBase
{
    public AttackStateMachine(StateMachineBase parentStateMachine) : base(parentStateMachine)
    {
        // TODO(Port): These states are far from completely configured.
        DefineState(AttackStateIds.ChaseTarget, new AttackMoveTowardsTargetState(this), StateId.ExitMachineWithFailure, StateId.ExitMachineWithFailure);
        DefineState(AttackStateIds.ApproachTarget, new AttackMoveTowardsTargetState(this), AttackStateIds.AimAtTarget, StateId.ExitMachineWithFailure);
        DefineState(AttackStateIds.AimAtTarget, new AttackAimWeaponState(this), AttackStateIds.FireWeapon, StateId.ExitMachineWithFailure);
        DefineState(AttackStateIds.FireWeapon, new AttackFireWeaponState(this), AttackStateIds.AimAtTarget, AttackStateIds.AimAtTarget);
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Persist(reader);
        reader.EndObject();
    }

    private sealed class AttackMoveTowardsTargetState : MoveTowardsState
    {
        private Vector3 _unknownPosition;
        private uint _unknownFrame;
        private bool _unknownBool1;
        private bool _unknownBool2;
        private bool _unknownBool3;
        private bool _unknownBool4;

        internal AttackMoveTowardsTargetState(AttackStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();

            reader.PersistVector3(ref _unknownPosition);
            reader.PersistFrame(ref _unknownFrame);
            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);
            reader.PersistBoolean(ref _unknownBool3);
            reader.PersistBoolean(ref _unknownBool4);
        }
    }

    private sealed class AttackAimWeaponState : State
    {
        private bool _unknownBool1;
        private bool _unknownBool2;

        internal AttackAimWeaponState(AttackStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);
        }
    }

    private sealed class AttackFireWeaponState : State
    {
        internal AttackFireWeaponState(AttackStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);
        }
    }
}
