#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI;

internal abstract class StateMachineBase : IPersistableObject, IDisposable
{
    public GameObject GameObject { get; }
    public IGameEngine GameEngine { get; }
    public virtual AIUpdate AIUpdate { get; }

    private readonly Dictionary<StateId, State> _states;

    private LogicFrame _sleepTill;

    private StateId _defaultStateId;

    private State? _currentState;

    public StateId CurrentStateId => _currentState?.Id ?? StateId.Invalid;

    public bool IsInIdleState => _currentState?.IsIdle ?? true; // Stateless things are considered idle.

    private ObjectId _goalObjectId;
    private Vector3 _goalPosition;
    private bool _locked;
    private bool _defaultStateInitialized;

    protected StateMachineBase(GameObject gameObject, IGameEngine gameEngine, AIUpdate aiUpdate)
    {
        GameObject = gameObject;
        GameEngine = gameEngine;
        AIUpdate = aiUpdate;
        _states = new Dictionary<StateId, State>();
    }

    protected StateMachineBase(StateMachineBase parent) : this(parent.GameObject, parent.GameEngine, parent.AIUpdate)
    {
    }

    public void Dispose()
    {
        _currentState?.OnExit(StateExitType.Reset);
    }

    /// <summary>
    /// Clears the state machine.
    /// </summary>
    private void Clear()
    {
        // If the machine is locked, it cannot be cleared.
        if (_locked)
        {
            // TODO(Port): Debug logging.
            return;
        }

        // Invoke the old state's OnExit().
        _currentState?.OnExit(StateExitType.Reset);

        _currentState = null;

        ClearInternal();
    }

    /// <summary>
    /// Clear the internal variables of the state machine to known values.
    /// </summary>
    private void ClearInternal()
    {
        _goalObjectId = ObjectId.Invalid;
        _goalPosition = Vector3.Zero;

        // TODO(Port): Debug logging.
    }

    /// <summary>
    /// Resets the machine to its default state.
    /// </summary>
    protected virtual StateReturnType ResetToDefaultState()
    {
        // If the machine is locked, it cannot be reset.
        if (_locked)
        {
            // TODO(Port): Debug logging.
            return StateReturnType.Failure;
        }

        if (!_defaultStateInitialized)
        {
            DebugUtility.Crash("You may not call ResetToDefaultState() before InitializeDefaultState()");
            return StateReturnType.Failure;
        }

        // Allow current state to exit with StateExitType.Reset if present.
        _currentState?.OnExit(StateExitType.Reset);
        _currentState = null;

        // The current state has done an OnExit. Clear the internal guts before
        // we set the new state. To clear it after the new state is set might be
        // overwriting things the new state transition causes to happen.
        ClearInternal();

        // Change to the default state.
        var status = SetStateInternal(_defaultStateId);

        DebugUtility.AssertCrash(status != StateReturnType.Failure, "Error setting default state");

        return status;
    }

    /// <summary>
    /// Given a unique (for this machine) ID number, and an instance of the
    /// <see cref="State"/> class, the machine records this as a possible state,
    /// and retains the ID mapping.
    ///
    /// These state IDs are used to change the machine's state via <see cref="SetState(StateId)"/>.
    /// </summary>
    protected void DefineState(StateId id, State state, StateId successId, StateId failureId, params ReadOnlySpan<StateConditionInfo> conditions)
    {
        _states.Add(id, state);

        // Store the ID in the state itself, as well.
        state.Id = id;

        state.OnSuccess(successId);
        state.OnFailure(failureId);

        foreach (var condition in conditions)
        {
            state.OnCondition(condition);
        }

        if (_defaultStateId == StateId.Invalid)
        {
            _defaultStateId = id;
        }
    }

    protected State GetState(StateId id)
    {
        if (_states.TryGetValue(id, out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"State {id} is not defined in {GetType().Name}");
    }

    internal StateReturnType SetState(StateId newStateId)
    {
        // If the machine is locked, it cannot change state via external events.
        if (_locked)
        {
            return StateReturnType.Continue;
        }

        return SetStateInternal(newStateId);
    }

    /// <summary>
    /// Changes the current state of the machine.
    /// This causes the old state's <see cref="State.OnExit(StateExitType)"/>
    /// to be invoked, and the new state's <see cref="State.OnEnter"/>
    /// to be invoked.
    /// </summary>
    internal StateReturnType SetStateInternal(StateId newStateId)
    {
        State? newState = null;

        // Anytime the state changes, stop sleeping.
        _sleepTill = LogicFrame.Zero;

        // If we're not setting the "done" state ID, we will continue with the
        // actual transition.
        if (newStateId != StateId.MachineDone)
        {
            // If incoming state is invalid, go to the machine's default state.
            if (newStateId == StateId.Invalid)
            {
                newStateId = _defaultStateId;
                if (newStateId == StateId.Invalid)
                {
                    DebugUtility.Crash("You may NEVER set the current state to an invalid state ID");
                    return StateReturnType.Failure;
                }
            }

            // Extract the state associated with the given ID.
            newState = GetState(newStateId);

            // TODO(Port): Debug logging.
        }

        // Invoke the old state's OnExit().
        _currentState?.OnExit(StateExitType.Normal);

        // Set the new state.
        _currentState = newState;

        return CallStateMethod(
            static state => state.OnEnter(),
            static () =>
            {
                // Irrelevant return code; we must return something.
                return StateReturnType.Continue;
            });
    }

    private StateReturnType CallStateMethod(
        Func<State, StateReturnType> stateMethod,
        Func<StateReturnType> noCurrentStateCallback)
    {
        if (_currentState != null)
        {
            // The state method (OnEnter() or Update()) could conceivably change
            // _currentState, so save it for a moment...
            var stateBeforeEnter = _currentState;

            var status = stateMethod(_currentState);

            // It is possible that the state's OnEnter() method may cause the
            // state to be destroyed.
            if (_currentState == null)
            {
                return StateReturnType.Failure;
            }

            // Here's the scenario:
            // 1. State A calls Foo() and then says "sleep for 2000 frames"
            // 2. However, Foo() called SetState() to State B. Thus our current
            //    state is not the same.
            // 3. Thus, if the state changed, we must ignore any sleep result
            //    and pretend we got StateReturnType.Continue so that the new
            //    state will be called immediately.
            if (stateBeforeEnter != _currentState)
            {
                status = StateReturnType.Continue;
            }

            if (status.Kind == StateReturnTypeKind.Sleep)
            {
                // Hey, we're sleepy!
                var now = GameEngine.GameLogic.CurrentFrame;
                _sleepTill = now + status.SleepForFrames;
                return _currentState.CheckForSleepTransitions(status);
            }
            else
            {
                // Check for state transitions, possibly exiting this machine.
                return _currentState.CheckForTransitions(status);
            }
        }
        else
        {
            return noCurrentStateCallback();
        }
    }

    /// <summary>
    /// Runs one step of the machine.
    /// </summary>
    internal virtual StateReturnType Update()
    {
        var now = GameEngine.GameLogic.CurrentFrame;
        if (_sleepTill != LogicFrame.Zero && now < _sleepTill)
        {
            if (_currentState == null)
            {
                return StateReturnType.Failure;
            }
            return _currentState.CheckForSleepTransitions(StateReturnType.Sleep(_sleepTill - now));
        }

        // Not sleeping anymore.
        _sleepTill = LogicFrame.Zero;

        return CallStateMethod(
            static state => state.Update(),
            static () =>
            {
                DebugUtility.Crash("State machine has no current state -- did you remember to call InitializeDefaultState()?");
                return StateReturnType.Failure;
            });
    }

    /// <summary>
    /// Defines the default state of the machine,
    /// and sets the machine's state to it.
    /// </summary>
    public StateReturnType InitializeDefaultState()
    {
        // TODO(Port): Debug logging.

        DebugUtility.AssertCrash(!_locked, "Machine is locked here, but probably should not be");

        if (_defaultStateInitialized)
        {
            DebugUtility.Crash("You may not call InitializeDefaultState() twice for the same StateMachine");
            return StateReturnType.Failure;
        }
        else
        {
            _defaultStateInitialized = true;
            return SetStateInternal(_defaultStateId);
        }
    }

    public bool IsGoalObjectDestroyed => _goalObjectId != ObjectId.Invalid && GoalObject == null;

    public void Halt()
    {
        _locked = true;

        // Don't exit current state, just clear it.
        _currentState = null;

        // TODO(Port): Debug logging.
    }

    public GameObject? GoalObject
    {
        get => GameEngine.GameLogic.GetObjectById(_goalObjectId);
        set
        {
            if (_locked)
            {
                return;
            }
            SetGoalObjectImpl(value);
        }
    }

    private void SetGoalObjectImpl(GameObject? obj)
    {
        if (obj != null)
        {
            _goalObjectId = obj.Id;
            SetGoalPositionImpl(obj.Translation);
        }
        else
        {
            _goalObjectId = ObjectId.Invalid;
        }
    }

    public Vector3 GoalPosition
    {
        get => _goalPosition;
        set
        {
            if (_locked)
            {
                return;
            }

            SetGoalPositionImpl(value);
        }
    }

    private void SetGoalPositionImpl(in Vector3 pos)
    {
        _goalPosition = pos;

        // Don't clear the goal object, or everything breaks.
        // Like construction of buildings.
    }

    public virtual void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistLogicFrame(ref _sleepTill);
        reader.PersistStateId(ref _defaultStateId);

        var currentStateId = CurrentStateId;
        reader.PersistStateId(ref currentStateId);
        if (reader.Mode == StatePersistMode.Read)
        {
            // We are going to jump into the current state.
            // We don't call OnEnter or OnExit, because
            // the state was already active when we saved.
            _currentState = GetState(currentStateId);
        }

        var snapshotAllStates = false;
#if DEBUG
        //snapshotAllStates = true;
#endif
        reader.PersistBoolean(ref snapshotAllStates);
        if (snapshotAllStates)
        {
            // Count all states in the mapping.
            // TODO(Port): Implement this.
        }
        else
        {
            DebugUtility.AssertCrash(_currentState != null, "Current state was null on xfer, trying to heal...");
            // Too late to find out why we are getting null in our state,
            // but if we let it go, we will crash in PersistObject.
            _currentState ??= GetState(_defaultStateId);
            reader.PersistObject(_currentState);
        }

        reader.PersistObjectId(ref _goalObjectId);
        reader.PersistVector3(ref _goalPosition);
        reader.PersistBoolean(ref _locked);
        reader.PersistBoolean(ref _defaultStateInitialized);
    }
}

public record struct StateId(uint Value)
{
    // Special arguments for OnCondition. It means when the given condition
    // becomes true, the state machine will exit and return the given status.
    public static readonly StateId ExitMachineWithSuccess = new(9998);
    public static readonly StateId ExitMachineWithFailure = new(9999);

    public static readonly StateId MachineDone = new(999998);
    public static readonly StateId Invalid = new(999999);
}
