#nullable enable

using System;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class DockState : State
{
    private readonly DockStateMachine _stateMachine;

    private bool _unknownBool2;

    public DockState(AIUpdateStateMachine stateMachine) : base(stateMachine)
    {
        _stateMachine = new DockStateMachine(stateMachine);
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        var unknownBool1 = true;
        reader.PersistBoolean(ref unknownBool1);
        if (!unknownBool1)
        {
            throw new InvalidStateException();
        }

        reader.PersistObject(_stateMachine);
        reader.PersistBoolean(ref _unknownBool2);
    }
}

internal sealed class DockStateMachine : StateMachineBase
{
    private static class DockStateIds
    {
        /// <summary>
        /// Given a queue position, move to it.
        /// </summary>
        public static readonly StateId Approach = new(0);

        /// <summary>
        /// Wait for dock to give us enter clearance.
        /// </summary>
        public static readonly StateId WaitForClearance = new(1);

        /// <summary>
        /// Advance in approach position as line move forward.
        /// </summary>
        public static readonly StateId AdvancePosition = new(2);

        /// <summary>
        /// Move to the dock entrance.
        /// </summary>
        public static readonly StateId MoveToEntry = new(3);

        /// <summary>
        /// Move to the actual dock position.
        /// </summary>
        public static readonly StateId MoveToDock = new(4);

        /// <summary>
        /// Invoke the dock's action until it is done.
        /// </summary>
        public static readonly StateId ProcessDock = new(5);

        /// <summary>
        /// Move to the dock exit, can exit the dock machine.
        /// </summary>
        public static readonly StateId MoveToExit = new(6);

        /// <summary>
        /// Move to rally if desired, exit the dock machine no matter what.
        /// </summary>
        public static readonly StateId MoveToRally = new(7);
    }

    private uint _unknownInt;

    public DockStateMachine(AIUpdateStateMachine parentStateMachine) : base(parentStateMachine)
    {
        ReadOnlySpan<StateConditionInfo> waitForClearanceConditions =
        [
            new StateConditionInfo(IsAbleToAdvance, DockStateIds.AdvancePosition),
        ];

        DefineState(DockStateIds.Approach, new DockApproachDockState(this), DockStateIds.WaitForClearance, StateId.ExitMachineWithFailure);
        DefineState(DockStateIds.WaitForClearance, new DockUnknown1State(this), DockStateIds.MoveToEntry, StateId.ExitMachineWithFailure, waitForClearanceConditions);
        DefineState(DockStateIds.MoveToEntry, new DockUnknown3State(this), DockStateIds.MoveToDock, DockStateIds.MoveToExit);
        DefineState(DockStateIds.MoveToDock, new DockUnknown4State(this), DockStateIds.ProcessDock, DockStateIds.MoveToExit);
        DefineState(DockStateIds.ProcessDock, new DockWaitForActionState(this), DockStateIds.MoveToExit, DockStateIds.MoveToExit);
    }

    private static bool IsAbleToAdvance(State state)
    {
        // TODO(Port): Implement this.
        throw new NotImplementedException();
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Persist(reader);
        reader.EndObject();

        reader.PersistUInt32(ref _unknownInt);
    }

    private sealed class DockApproachDockState : MoveTowardsState
    {
        internal DockApproachDockState(DockStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();
        }
    }

    private sealed class DockUnknown1State : State
    {
        private uint _unknownInt;

        internal DockUnknown1State(DockStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.PersistUInt32(ref _unknownInt);
        }
    }

    private sealed class DockUnknown3State : MoveTowardsState
    {
        internal DockUnknown3State(DockStateMachine stateMachine) : base(stateMachine)
        {
        }
    }

    private sealed class DockUnknown4State : MoveTowardsState
    {
        internal DockUnknown4State(DockStateMachine stateMachine) : base(stateMachine)
        {
        }
    }

    private sealed class DockWaitForActionState : State
    {
        // Time spent in this state matches SupplyWarehouseActionDelay

        internal DockWaitForActionState(DockStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);
        }
    }
}
