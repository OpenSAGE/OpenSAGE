using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenSage.Logic.AI
{
    internal abstract class StateMachineBase
    {
        private readonly Dictionary<uint, State> _states;

        private uint _unknownFrame;
        private uint _unknownInt1;

        private uint _currentStateId;
        private State _currentState;

        private uint _unknownInt2;
        private Vector3 _unknownPosition;
        private bool _unknownBool1;
        private bool _unknownBool2;

        protected StateMachineBase()
        {
            _states = new Dictionary<uint, State>();
        }

        public void AddState(uint id, State state)
        {
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

        internal virtual void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistFrame(ref _unknownFrame);
            reader.PersistUInt32(ref _unknownInt1);

            reader.PersistUInt32(ref _currentStateId);
            _currentState = GetState(_currentStateId);

            reader.SkipUnknownBytes(1);

            _currentState.Load(reader);

            reader.PersistUInt32(ref _unknownInt2);
            reader.PersistVector3(ref _unknownPosition);
            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);
        }
    }
}
