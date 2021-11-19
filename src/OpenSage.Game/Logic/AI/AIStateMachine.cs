using System.IO;
using OpenSage.Data.Sav;
using OpenSage.Logic.AI.AIStates;

namespace OpenSage.Logic.AI
{
    internal sealed class AIStateMachine : StateMachineBase
    {
        public AIStateMachine()
        {
            AddState(0, new IdleAIState());
            AddState(1, new MoveTowardsAIState());
            AddState(3, new AIState3());
            AddState(5, new AIState5());
            AddState(6, new AIState6());
            AddState(9, new AIState11());
            AddState(11, new AIState11());
            AddState(14, new AIState14());
            AddState(16, new AIState16());
            AddState(18, new AIState18());
            AddState(32, new AIState32());
            AddState(33, new FaceAIState(FaceAIStateType.FaceNamed));
            AddState(34, new FaceAIState(FaceAIStateType.FaceWaypoint));
            AddState(40, new WanderInPlaceAIState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    internal class AIState3 : MoveTowardsAIState
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

    internal sealed class AIState5 : MoveTowardsAIState
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var waypointIdMaybe = reader.ReadUInt32();
        }
    }

    internal sealed class AIState6 : MoveTowardsAIState
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
            AddState(2, new AIState11StateMachineState2());
            AddState(3, new AIState11StateMachineState3());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    internal sealed class AIState11StateMachineState1 : MoveTowardsAIState
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

    internal sealed class AIState11StateMachineState2 : State
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool1 = reader.ReadBoolean();
            var unknownBool2 = reader.ReadBoolean();
        }
    }

    internal sealed class AIState11StateMachineState3 : State
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);
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
            AddState(3, new AIState14StateMachineState3());
            AddState(4, new AIState14StateMachineState4());
            AddState(5, new AIState14StateMachineState5());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknownInt1 = reader.ReadUInt32();
        }
    }

    internal sealed class AIState14StateMachineState0 : MoveTowardsAIState
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            base.Load(reader);
        }
    }

    internal sealed class AIState14StateMachineState3 : MoveTowardsAIState
    {
    }

    internal sealed class AIState14StateMachineState4 : MoveTowardsAIState
    {
    }

    internal sealed class AIState14StateMachineState5 : State
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);
        }
    }

    internal sealed class AIState16 : State
    {
        private readonly AIState16StateMachine _stateMachine;

        public AIState16()
        {
            _stateMachine = new AIState16StateMachine();
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool1 = reader.ReadBoolean();

            _stateMachine.Load(reader);
        }
    }

    internal sealed class AIState16StateMachine : StateMachineBase
    {
        public AIState16StateMachine()
        {
            AddState(5001, new AIState16StateMachineState5001());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            base.Load(reader);

            var unknownInt1 = reader.ReadUInt32();
            if (unknownInt1 != 0)
            {
                throw new InvalidDataException();
            }

            var unknownInt2 = reader.ReadUInt32();
            var unknownPos = reader.ReadVector3();

            var unknownByte = reader.ReadByte();
            if (unknownByte != 0)
            {
                throw new InvalidDataException();
            }
        }
    }

    internal sealed class AIState16StateMachineState5001 : State
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownInt1 = reader.ReadUInt32();
        }
    }

    internal sealed class AIState18 : AIState3
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknownInt0 = reader.ReadUInt32();
            var unknownInt1 = reader.ReadUInt32();
        }
    }

    internal sealed class AIState32 : AIState3
    {
        private readonly AIState32StateMachine _stateMachine;

        public AIState32()
        {
            _stateMachine = new AIState32StateMachine();
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            _stateMachine.Load(reader);
        }
    }

    internal sealed class AIState32StateMachine : StateMachineBase
    {
        public AIState32StateMachine()
        {
            AddState(0, new IdleAIState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }
}
