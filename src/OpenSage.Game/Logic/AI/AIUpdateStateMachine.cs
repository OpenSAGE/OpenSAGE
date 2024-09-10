using System.Collections.Generic;
using System.Numerics;
using OpenSage.Logic.AI.AIStates;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI
{
    internal class AIUpdateStateMachine : StateMachineBase
    {
        private readonly GameObject _gameObject;
        private readonly GameContext _context;
        private readonly List<Vector3> _targetPositions = new();
        private string _targetWaypointName;
        private TargetTeam _targetTeam;

        private State _overrideState;
        private LogicFrame _overrideStateUntilFrame;

        public AIUpdateStateMachine(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            AddState(IdleState.StateId, new IdleState(gameObject, context));
            AddState(1, new MoveTowardsState(gameObject, context, aiUpdate));
            AddState(2, new FollowWaypointsState(gameObject, context, aiUpdate, true));
            AddState(3, new FollowWaypointsState(gameObject, context, aiUpdate, false));
            AddState(4, new FollowWaypointsExactState(gameObject, context, aiUpdate, true));
            AddState(5, new FollowWaypointsExactState(gameObject, context, aiUpdate, false));
            AddState(6, new AIState6(gameObject, context, aiUpdate));
            AddState(7, new FollowPathState(gameObject, context, aiUpdate));
            AddState(9, new AttackState(gameObject, context, aiUpdate));
            AddState(10, new AttackState(gameObject, context, aiUpdate));
            AddState(11, new AttackState(gameObject, context, aiUpdate));
            AddState(13, new DeadState(gameObject, context));
            AddState(14, new DockState(gameObject, context, aiUpdate));
            AddState(15, new EnterContainerState(gameObject, context, aiUpdate));
            AddState(16, new GuardState(gameObject, context, aiUpdate));
            AddState(17, new HuntState(gameObject, context, aiUpdate));
            AddState(18, new WanderState(gameObject, context, aiUpdate));
            AddState(19, new PanicState(gameObject, context, aiUpdate));
            AddState(20, new AttackTeamState(gameObject, context, aiUpdate));
            AddState(21, new GuardInTunnelNetworkState(gameObject, context, aiUpdate));
            AddState(23, new AIState23(gameObject, context, aiUpdate));
            AddState(28, new AttackAreaState(gameObject, context, aiUpdate));
            AddState(30, new AttackMoveState(gameObject, context, aiUpdate));
            AddState(32, new AIState32(gameObject, context, aiUpdate));
            AddState(33, new FaceState(gameObject, context, FaceTargetType.FaceNamed));
            AddState(34, new FaceState(gameObject, context, FaceTargetType.FaceWaypoint));
            AddState(35, new RappelState(gameObject, context));
            AddState(37, new ExitContainerState(gameObject, context));
            AddState(40, new WanderInPlaceState(gameObject, context, aiUpdate));
            AddState(41, new DoNothingState(gameObject, context));
        }

        internal override void Update()
        {
            if (_overrideState != null)
            {
                var overrideStateResult = _overrideState.Update();

                var currentFrame = _context.GameLogic.CurrentFrame;

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

        internal AIState6(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context, aiUpdate)
        {
        }

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
        internal AIState23(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context, aiUpdate)
        {
        }
    }

    internal sealed class AIState32 : FollowWaypointsState
    {
        private readonly AIState32StateMachine _stateMachine;

        public AIState32(GameObject gameObject, GameContext context, AIUpdate aiUpdate)
            : base(gameObject, context, aiUpdate, false)
        {
            _stateMachine = new AIState32StateMachine(gameObject, context, aiUpdate);
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
        public AIState32StateMachine(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            AddState(IdleState.StateId, new IdleState(gameObject, context));
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
