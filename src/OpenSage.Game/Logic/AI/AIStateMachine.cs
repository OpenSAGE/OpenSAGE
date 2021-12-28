using System.Collections.Generic;
using System.Numerics;
using OpenSage.Logic.AI.AIStates;

namespace OpenSage.Logic.AI
{
    internal sealed class AIStateMachine : StateMachineBase
    {
        private readonly List<Vector3> _targetPositions = new();
        private string _targetWaypointName;
        private TargetTeam _targetTeam;
        private State _stateSomething;

        public AIStateMachine()
        {
            AddState(0, new IdleState());
            AddState(1, new MoveTowardsState());
            AddState(2, new FollowWaypointsState(true));
            AddState(3, new FollowWaypointsState(false));
            AddState(4, new FollowWaypointsExactState(true));
            AddState(5, new FollowWaypointsExactState(false));
            AddState(6, new AIState6());
            AddState(7, new FollowPathState());
            AddState(9, new AttackState());
            AddState(10, new AttackState());
            AddState(11, new AttackState());
            AddState(13, new DeadState());
            AddState(14, new DockState());
            AddState(15, new EnterContainerState());
            AddState(16, new GuardState());
            AddState(17, new HuntState());
            AddState(18, new WanderState());
            AddState(19, new PanicState());
            AddState(20, new AttackTeamState());
            AddState(21, new GuardInTunnelNetworkState());
            AddState(23, new AIState23());
            AddState(28, new AttackAreaState());
            AddState(30, new AttackMoveState());
            AddState(32, new AIState32());
            AddState(33, new FaceState(FaceTargetType.FaceNamed));
            AddState(34, new FaceState(FaceTargetType.FaceWaypoint));
            AddState(37, new ExitContainerState());
            AddState(40, new WanderInPlaceState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var numTargetPositions = reader.ReadUInt32();
            for (var i = 0; i < numTargetPositions; i++)
            {
                Vector3 targetPosition = default;
                reader.ReadVector3(ref targetPosition);
                _targetPositions.Add(targetPosition);
            }

            _targetWaypointName = reader.ReadAsciiString();

            var hasTargetTeam = reader.ReadBoolean();
            if (hasTargetTeam)
            {
                _targetTeam ??= new TargetTeam();
                _targetTeam.Load(reader);
            }

            var stateSomethingId = reader.ReadUInt32();
            if (stateSomethingId != 999999)
            {
                _stateSomething = GetState(stateSomethingId);
                _stateSomething.Load(reader);
            }
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

    internal sealed class AIState23 : MoveTowardsState
    {

    }

    internal sealed class AIState32 : FollowWaypointsState
    {
        private readonly AIState32StateMachine _stateMachine;

        public AIState32()
            : base(false)
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

    internal sealed class TargetTeam
    {
        private readonly List<uint> _objectIds = new();

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var numTeamObjects = reader.ReadUInt16();
            for (var i = 0; i < numTeamObjects; i++)
            {
                uint objectId = 0;
                reader.ReadObjectID(ref objectId);
                _objectIds.Add(objectId);
            }
        }
    }
}
