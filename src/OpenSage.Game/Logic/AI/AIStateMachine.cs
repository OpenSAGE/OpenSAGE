using OpenSage.Data.Sav;
using OpenSage.Logic.AI.AIStates;

namespace OpenSage.Logic.AI
{
    internal sealed class AIStateMachine : StateMachineBase
    {
        public AIStateMachine()
        {
            AddState(0, new IdleState());
            AddState(1, new MoveTowardsState());
            AddState(3, new AIState3());
            AddState(5, new AIState5());
            AddState(6, new AIState6());
            AddState(9, new AttackState());
            AddState(11, new AttackState());
            AddState(13, new DeadState());
            AddState(14, new AIState14());
            AddState(16, new GuardState());
            AddState(18, new AIState18());
            AddState(28, new AttackAreaState());
            AddState(32, new AIState32());
            AddState(33, new FaceState(FaceTargetType.FaceNamed));
            AddState(34, new FaceState(FaceTargetType.FaceWaypoint));
            AddState(40, new WanderInPlaceState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    internal class AIState3 : MoveTowardsState
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

    internal sealed class AIState5 : MoveTowardsState
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var waypointIdMaybe = reader.ReadUInt32();
        }
    }

    internal sealed class AIState6 : MoveTowardsState
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

    internal sealed class AIState14StateMachineState0 : MoveTowardsState
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            base.Load(reader);
        }
    }

    internal sealed class AIState14StateMachineState3 : MoveTowardsState
    {
    }

    internal sealed class AIState14StateMachineState4 : MoveTowardsState
    {
    }

    internal sealed class AIState14StateMachineState5 : State
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);
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
            AddState(0, new IdleState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }
}
