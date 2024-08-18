using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI
{
    internal abstract class StateMachineBase : IPersistableObject
    {
        private readonly Dictionary<uint, State> _states;

        private uint _unknownFrame;
        private uint _unknownInt1;

        private uint _currentStateId;
        private State _currentState;
        internal State CurrentState => _currentState;

        private uint _targetObjectId;
        private Vector3 _targetPosition;
        private bool _unknownBool1;
        private bool _unknownBool2;

        public readonly GameObject GameObject;

        protected StateMachineBase()
        {
            _states = new Dictionary<uint, State>();
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
            _currentState?.OnExit();

            _currentState = GetState(id);

            _currentState.OnEnter();
        }

        internal virtual void Update()
        {
            if (_currentState == null)
            {
                return;
            }

            var updateResult = _currentState.Update();

            switch (updateResult.Type)
            {
                case UpdateStateResultType.Continue:
                    break;

                case UpdateStateResultType.TransitionToState:
                    SetState(updateResult.TransitionToStateId.Value);
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
            _currentState = GetState(_currentStateId);

            reader.SkipUnknownBytes(1);

            reader.PersistObject(_currentState);
            reader.PersistObjectID(ref _targetObjectId);
            reader.PersistVector3(ref _targetPosition);
            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);
        }
    }
}
