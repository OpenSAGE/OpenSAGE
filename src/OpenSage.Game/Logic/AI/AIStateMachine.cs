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

        private uint _stateSomethingId;
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

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();

            reader.PersistListWithUInt32Count("TargetPositions", _targetPositions, static (StatePersister persister, ref Vector3 item) =>
            {
                persister.PersistVector3Value(ref item);
            });

            reader.PersistAsciiString("TargetWaypointName", ref _targetWaypointName);

            var hasTargetTeam = _targetTeam != null;
            reader.PersistBoolean("HasTargetTeam", ref hasTargetTeam);
            if (hasTargetTeam)
            {
                _targetTeam ??= new TargetTeam();
                reader.PersistObject("TargetTeam", _targetTeam);
            }

            reader.PersistUInt32("StateSomethingId", ref _stateSomethingId);
            if (_stateSomethingId != 999999)
            {
                _stateSomething = GetState(_stateSomethingId);
                reader.PersistObject("StateSomething", _stateSomething);
            }
        }
    }

    internal sealed class AIState6 : MoveTowardsState
    {
        private int _unknownInt;
        private bool _unknownBool1;
        private bool _unknownBool2;

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Persist(reader);

            reader.PersistInt32("UnknownInt", ref _unknownInt);
            reader.PersistBoolean("UnknownBool1", ref _unknownBool1);
            reader.PersistBoolean("UnknownBool2", ref _unknownBool2);
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

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Persist(reader);

            reader.PersistObject("StateMachine", _stateMachine);
        }
    }

    internal sealed class AIState32StateMachine : StateMachineBase
    {
        public AIState32StateMachine()
        {
            AddState(0, new IdleState());
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Persist(reader);
        }
    }

    internal sealed class TargetTeam : IPersistableObject
    {
        private readonly List<uint> _objectIds = new();

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistList("ObjectIds", _objectIds, static (StatePersister persister, ref uint item) =>
            {
                persister.PersistObjectIDValue(ref item);
            });
        }
    }
}
