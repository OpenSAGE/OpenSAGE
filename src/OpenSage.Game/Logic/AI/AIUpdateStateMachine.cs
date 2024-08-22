using System.Collections.Generic;
using System.Numerics;
using OpenSage.Logic.AI.AIStates;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI
{
    internal class AIUpdateStateMachine : StateMachineBase
    {
        private readonly List<Vector3> _targetPositions = new();
        private string _targetWaypointName;
        private TargetTeam _targetTeam;

        private State _overrideState;
        private LogicFrame _overrideStateUntilFrame;

        public AIUpdateStateMachine(GameObject gameObject)
        {
            AddState(IdleState.StateId, new IdleState());
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
            AddState(35, new RappelState());
            AddState(37, new ExitContainerState());
            AddState(40, new WanderInPlaceState());
            AddState(41, new DoNothingState());
        }

        internal override void Update()
        {
            if (_overrideState != null)
            {
                var overrideStateResult = _overrideState.Update();

                var currentFrame = GameObject.GameContext.GameLogic.CurrentFrame;

                var shouldContinueOverrideState = overrideStateResult.Type switch
                {
                    UpdateStateResultType.Continue => _overrideStateUntilFrame >= currentFrame,
                    UpdateStateResultType.TransitionToState => false,
                    _ => throw new System.InvalidOperationException(),
                };

                if (shouldContinueOverrideState)
                {
                    return;
                }

                _overrideState.OnExit();
                _overrideState = null;
            }

            base.Update();
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();

            reader.PersistListWithUInt32Count(
                _targetPositions,
                static (StatePersister persister, ref Vector3 item) =>
                {
                    persister.PersistVector3Value(ref item);
                });

            reader.PersistAsciiString(ref _targetWaypointName);

            var hasTargetTeam = _targetTeam != null;
            reader.PersistBoolean(ref hasTargetTeam);
            if (hasTargetTeam)
            {
                _targetTeam ??= new TargetTeam();
                reader.PersistObject(_targetTeam);
            }

            const uint unsetOverrideStateId = 999999;
            var overrideStateId = unsetOverrideStateId;
            if (reader.Mode == StatePersistMode.Write && _overrideState != null)
            {
                overrideStateId = _overrideState.Id;
            }
            reader.PersistUInt32(ref overrideStateId);
            if (reader.Mode == StatePersistMode.Read && overrideStateId != unsetOverrideStateId)
            {
                _overrideState = GetState(overrideStateId);
            }
            if (_overrideState != null)
            {
                reader.PersistObject(_overrideState);
            }

            reader.PersistLogicFrame(ref _overrideStateUntilFrame);
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

            reader.PersistInt32(ref _unknownInt);
            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);
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

            reader.PersistObject(_stateMachine);
        }
    }

    internal sealed class AIState32StateMachine : StateMachineBase
    {
        public AIState32StateMachine()
        {
            AddState(IdleState.StateId, new IdleState());
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

            reader.PersistList(
                _objectIds,
                static (StatePersister persister, ref uint item) =>
            {
                persister.PersistObjectIDValue(ref item);
            });
        }
    }
}
