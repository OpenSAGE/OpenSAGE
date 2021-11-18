using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI
{
    internal abstract class StateMachineBase
    {
        private readonly Dictionary<int, State> _states;
        private State _currentState;

        protected StateMachineBase()
        {
            _states = new Dictionary<int, State>();
        }

        public void AddState(int id, State state)
        {
            _states.Add(id, state);
        }

        internal virtual void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var frameSomething2 = reader.ReadUInt32();
            var unknownInt4 = reader.ReadUInt32();

            var currentStateID = reader.ReadUInt32();
            _currentState = _states[(int)currentStateID];

            var unknownBool1 = reader.ReadBoolean();
            if (unknownBool1)
            {
                throw new InvalidDataException();
            }

            _currentState.Load(reader);

            var unknownInt9 = reader.ReadUInt32();
            var positionSomething3 = reader.ReadVector3();
            var unknownBool4 = reader.ReadBoolean();
            var unknownBool5 = reader.ReadBoolean();
        }
    }
}
