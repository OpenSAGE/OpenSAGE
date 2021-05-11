using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI
{
    internal sealed class AIStateMachine : StateMachineBase
    {
        public AIStateMachine()
        {
            AddState(0, new AIState0());
            AddState(1, new AIState1());
            AddState(3, new AIState3());
            AddState(6, new AIState6());
            AddState(11, new AIState11());
            AddState(14, new AIState14());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    internal sealed class AIState0 : State
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownShort1 = reader.ReadUInt16();
            var unknownShort2 = reader.ReadUInt16();
        }
    }

    internal class AIState1 : State
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var positionSomething = reader.ReadVector3();
            var unknownInt1 = reader.ReadUInt32();
            var unknownBool1 = reader.ReadBoolean();
            var positionSomething2 = reader.ReadVector3();
            var unknownInt2 = reader.ReadUInt32();
            var unknownInt3 = reader.ReadUInt32();
            var unknownBool2 = reader.ReadBoolean();
        }
    }

    internal sealed class AIState3 : AIState1
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknownInt0 = reader.ReadUInt32();
            var unknownInt1 = reader.ReadUInt32();
            var unknownInt2 = reader.ReadUInt32();
            var unknownInt3 = reader.ReadUInt32();
            var waypointIdMaybe = reader.ReadUInt32();
            var waypointId2Maybe = reader.ReadUInt32();
            var unknownBool1 = reader.ReadBoolean();
        }
    }

    internal sealed class AIState6 : AIState1
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknownInt1 = reader.ReadInt32();
            var unknownBool1 = reader.ReadBoolean();
            var unknownBool2 = reader.ReadBoolean();
        }
    }

    internal sealed class AIState11 : State
    {
        private readonly AIState11StateMachine _stateMachine;

        public AIState11()
        {
            _stateMachine = new AIState11StateMachine();
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool2 = reader.ReadBoolean();
            var positionSomething = reader.ReadVector3();

            _stateMachine.Load(reader);
        }
    }

    internal sealed class AIState11StateMachine : StateMachineBase
    {
        public AIState11StateMachine()
        {
            AddState(1, new AIState11StateMachineState1());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    internal sealed class AIState11StateMachineState1 : AIState1
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var positionSomething2 = reader.ReadVector3();
            var frameSomething = reader.ReadUInt32();
            var unknownBool1 = reader.ReadBoolean();
            var unknownBool2 = reader.ReadBoolean();
            var unknownBool3 = reader.ReadBoolean();
            var unknownBool4 = reader.ReadBoolean();
        }
    }

    internal sealed class AIState14 : State
    {
        private readonly AIState14StateMachine _stateMachine;

        public AIState14()
        {
            _stateMachine = new AIState14StateMachine();
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool1 = reader.ReadBoolean();

            _stateMachine.Load(reader);

            var unknownBool2 = reader.ReadBoolean();
        }
    }

    internal sealed class AIState14StateMachine : StateMachineBase
    {
        public AIState14StateMachine()
        {
            AddState(0, new AIState14StateMachineState0());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknownInt1 = reader.ReadUInt32();
        }
    }

    internal sealed class AIState14StateMachineState0 : AIState1
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            base.Load(reader);
        }
    }

    internal abstract class State
    {
        internal abstract void Load(SaveFileReader reader);
    }

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
