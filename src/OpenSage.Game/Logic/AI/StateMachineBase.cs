using System;
using System.Collections.Generic;
using System.IO;

namespace OpenSage.Logic.AI
{
    internal abstract class StateMachineBase
    {
        private readonly Dictionary<uint, State> _states;
        private State _currentState;

        protected StateMachineBase()
        {
            _states = new Dictionary<uint, State>();
        }

        public void AddState(uint id, State state)
        {
            _states.Add(id, state);
        }

        public State GetState(uint id)
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

            var frameSomething2 = reader.ReadUInt32();
            var unknownInt4 = reader.ReadUInt32();

            var currentStateID = reader.ReadUInt32();
            _currentState = GetState(currentStateID);

            reader.SkipUnknownBytes(1);

            _currentState.Load(reader);

            var unknownInt9 = reader.ReadUInt32();
            var positionSomething3 = reader.ReadVector3();
            var unknownBool4 = reader.ReadBoolean();
            var unknownBool5 = reader.ReadBoolean();
        }
    }
}
