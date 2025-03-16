#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI;

internal abstract class StateMachineBase : IPersistableObject
{
    public GameObject GameObject { get; }
    public GameEngine GameEngine { get; }
    public virtual AIUpdate AIUpdate { get; }

    private readonly Dictionary<uint, State> _states;

    private uint _unknownFrame;
    private uint _unknownInt1;

    private uint _currentStateId;
    internal State? CurrentState { get; private set; }

    private uint _targetObjectId;
    private Vector3 _targetPosition;
    private bool _unknownBool1;
    private bool _unknownBool2;

    protected StateMachineBase(GameObject gameObject, GameEngine gameEngine, AIUpdate aiUpdate)
    {
        GameObject = gameObject;
        GameEngine = gameEngine;
        AIUpdate = aiUpdate;
        _states = new Dictionary<uint, State>();
    }

    protected StateMachineBase(StateMachineBase parent) : this(parent.GameObject, parent.GameEngine, parent.AIUpdate)
    {
    }

    public void AddState(uint id, State state)
    {
        state.Id = id;

        _states.Add(id, state);
    }

    protected State GetState(uint id)
    {
        if (_states.TryGetValue(id, out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"State {id} is not defined in {GetType().Name}");
    }

    internal void SetState(uint id)
    {
        CurrentState?.OnExit();

        CurrentState = GetState(id);

        CurrentState.OnEnter();
    }

    internal virtual void Update()
    {
        if (CurrentState == null)
        {
            return;
        }

        var updateResult = CurrentState.Update();

        switch (updateResult.Type)
        {
            case UpdateStateResultType.Continue:
                break;

            case UpdateStateResultType.TransitionToState:
                SetState(updateResult.TransitionToStateId ?? throw new InvalidStateException());
                break;

            default:
                break;
        }
    }

    public virtual void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistFrame(ref _unknownFrame);
        reader.PersistUInt32(ref _unknownInt1);

        reader.PersistUInt32(ref _currentStateId);
        CurrentState = GetState(_currentStateId);

        reader.SkipUnknownBytes(1);

        reader.PersistObject(CurrentState);
        reader.PersistObjectID(ref _targetObjectId);
        reader.PersistVector3(ref _targetPosition);
        reader.PersistBoolean(ref _unknownBool1);
        reader.PersistBoolean(ref _unknownBool2);
    }
}
