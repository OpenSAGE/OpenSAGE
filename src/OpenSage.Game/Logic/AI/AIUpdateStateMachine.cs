using System.Collections.Generic;
using System.Numerics;
using OpenSage.Logic.AI.AIStates;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI;

internal class AIUpdateStateMachine : StateMachineBase
{
    private readonly List<Vector3> _targetPositions = new();
    private string _targetWaypointName;
    private TargetTeam _targetTeam;

    private State _overrideState;
    private LogicFrame _overrideStateUntilFrame;

    public AIUpdateStateMachine(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context, aiUpdate)
    {
        AddState(IdleState.StateId, new IdleState(this));
        AddState(1, new MoveTowardsState(this));
        AddState(2, new FollowWaypointsState(this, true));
        AddState(3, new FollowWaypointsState(this, false));
        AddState(4, new FollowWaypointsExactState(this, true));
        AddState(5, new FollowWaypointsExactState(this, false));
        AddState(6, new AIState6(this));
        AddState(7, new FollowPathState(this));
        AddState(9, new AttackState(this));
        AddState(10, new AttackState(this));
        AddState(11, new AttackState(this));
        AddState(13, new DeadState(this));
        AddState(14, new DockState(this));
        AddState(15, new EnterContainerState(this));
        AddState(16, new GuardState(this));
        AddState(17, new HuntState(this));
        AddState(18, new WanderState(this));
        AddState(19, new PanicState(this));
        AddState(20, new AttackTeamState(this));
        AddState(21, new GuardInTunnelNetworkState(this));
        AddState(23, new AIState23(this));
        AddState(28, new AttackAreaState(this));
        AddState(30, new AttackMoveState(this));
        AddState(32, new AIState32(this));
        AddState(33, new FaceState(this, FaceTargetType.FaceNamed));
        AddState(34, new FaceState(this, FaceTargetType.FaceWaypoint));
        AddState(35, new RappelState(this));
        AddState(37, new ExitContainerState(this));
        AddState(40, new WanderInPlaceState(this));
        AddState(41, new DoNothingState(this));
    }

    internal override void Update()
    {
        if (_overrideState != null)
        {
            var overrideStateResult = _overrideState.Update();

            var currentFrame = Context.GameLogic.CurrentFrame;

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
