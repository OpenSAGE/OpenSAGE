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

        internal virtual void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            reader.ReadFrame(ref _unknownFrame);
            _unknownInt1 = reader.ReadUInt32();

            var currentStateID = reader.ReadUInt32();
            _currentState = GetState(currentStateID);

            reader.SkipUnknownBytes(1);

            _currentState.Load(reader);

            _unknownInt2 = reader.ReadUInt32();
            reader.ReadVector3(ref _unknownPosition);
            _unknownBool1 = reader.ReadBoolean();
            _unknownBool2 = reader.ReadBoolean();
        }
    }
}
