#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI;

public abstract class State : IPersistableObject
{
    private StateId _successStateId;
    private StateId _failureStateId;

    private readonly List<StateConditionInfo> _conditions = new();

    public StateId Id { get; internal set; }

    private protected GameObject GameObject => _stateMachine.GameObject;
    private protected IGameEngine GameEngine => _stateMachine.GameEngine;

    private readonly StateMachineBase _stateMachine;

    public virtual bool IsIdle => false;

    private protected State(StateMachineBase stateMachine)
    {
        _stateMachine = stateMachine;
    }

    internal void OnSuccess(StateId toStateId)
    {
        _successStateId = toStateId;
    }

    internal void OnFailure(StateId toStateId)
    {
        _failureStateId = toStateId;
    }

    internal void OnCondition(StateConditionInfo condition)
    {
        _conditions.Add(condition);
    }

    /// <summary>
    /// Executed once when entering state.
    /// </summary>
    public virtual StateReturnType OnEnter() => StateReturnType.Continue;

    /// <summary>
    /// Executed once when leaving state.
    /// </summary>
    public virtual void OnExit(StateExitType status) { }

    /// <summary>
    /// Given a return code, handle state transitions.
    /// </summary>
    internal StateReturnType CheckForTransitions(StateReturnType status)
    {
        DebugUtility.AssertCrash(status.Kind != StateReturnTypeKind.Sleep, "Please handle sleep states prior to this!");

        // Handle transitions.
        switch (status.Kind)
        {
            case StateReturnTypeKind.Success:
                return CheckForTransitionSuccessOrFailure(_successStateId);

            case StateReturnTypeKind.Failure:
                return CheckForTransitionSuccessOrFailure(_failureStateId);

            case StateReturnTypeKind.Continue:
                if (CheckTransitionConditions(out var result))
                {
                    return result;
                }
                break;
        }

        // The machine keeps running.
        return StateReturnType.Continue;
    }

    private StateReturnType CheckForTransitionSuccessOrFailure(StateId stateId)
    {
        if (stateId == StateId.ExitMachineWithSuccess)
        {
            _stateMachine.SetStateInternal(StateId.MachineDone);
            return StateReturnType.Success;
        }
        else if (stateId == StateId.ExitMachineWithFailure)
        {
            _stateMachine.SetStateInternal(StateId.MachineDone);
            return StateReturnType.Failure;
        }

        // Move to new state.
        return _stateMachine.SetStateInternal(stateId);
    }

    private bool CheckTransitionConditions(out StateReturnType result)
    {
        foreach (var condition in _conditions)
        {
            if (!condition.Test(this))
            {
                continue;
            }

            // Test returned true, change to associated state.

            // TODO(Port): Debug logging.

            // Check if machine should exit.
            if (condition.ToStateId == StateId.ExitMachineWithSuccess)
            {
                result = StateReturnType.Success;
                return true;
            }
            else if (condition.ToStateId == StateId.ExitMachineWithFailure)
            {
                result = StateReturnType.Failure;
                return true;
            }
            else
            {
                // Move to new state.
                result = _stateMachine.SetStateInternal(condition.ToStateId);
                return true;
            }
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Given a return code, handle state transitions while state is sleeping.
    /// </summary>
    internal StateReturnType CheckForSleepTransitions(StateReturnType status)
    {
        DebugUtility.AssertCrash(status.Kind == StateReturnTypeKind.Sleep, "Please only pass sleep states here");

        if (CheckTransitionConditions(out var result))
        {
            return result;
        }

        return status;
    }

    // TODO(Port): Make this abstract.
    public virtual StateReturnType Update() => StateReturnType.Continue;

    public abstract void Persist(StatePersister reader);
}

public readonly struct StateReturnType
{
    public static readonly StateReturnType Continue = new(StateReturnTypeKind.Continue, null);
    public static readonly StateReturnType Success = new(StateReturnTypeKind.Success, null);
    public static readonly StateReturnType Failure = new(StateReturnTypeKind.Failure, null);
    public static StateReturnType Sleep(LogicFrameSpan forFrames) => new(StateReturnTypeKind.Sleep, forFrames);

    // We use 0x3fffffff so that we can add offsets and not overflow...
    // At 30fps that's around ~414 days.
    public static readonly StateReturnType SleepForever = Sleep(new LogicFrameSpan(0x3fffffff));

    public readonly StateReturnTypeKind Kind;

    public readonly LogicFrameSpan SleepForFrames;

    private StateReturnType(StateReturnTypeKind kind, LogicFrameSpan? sleepForFrames)
    {
        Kind = kind;
        SleepForFrames = sleepForFrames ?? LogicFrameSpan.Zero;
    }

    public override bool Equals(object? obj)
    {
        return obj is StateReturnType other && this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Kind, SleepForFrames);
    }

    public static bool operator ==(StateReturnType left, StateReturnType right)
    {
        return left.Kind == right.Kind && left.SleepForFrames == right.SleepForFrames;
    }

    public static bool operator !=(StateReturnType left, StateReturnType right)
    {
        return !(left == right);
    }
}

public enum StateReturnTypeKind
{
    Continue,
    Success,
    Failure,
    Sleep,
}

public enum StateExitType
{
    /// <summary>
    /// State exited due to normal state transitioning.
    /// </summary>
    Normal,

    /// <summary>
    /// State exited due to state machine reset.
    /// </summary>
    Reset,
}

public readonly record struct StateConditionInfo(StateTransitionDelegate Test, StateId ToStateId);

public delegate bool StateTransitionDelegate(State state);
