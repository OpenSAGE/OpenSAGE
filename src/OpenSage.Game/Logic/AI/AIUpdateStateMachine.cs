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

    private State _temporaryState;
    private LogicFrame _temporaryStateFrameEnd;

    public AIUpdateStateMachine(GameObject gameObject, IGameEngine gameEngine, AIUpdate aiUpdate) : base(gameObject, gameEngine, aiUpdate)
    {
        DefineState(AIStateIds.Idle, new IdleState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.MoveTo, new MoveTowardsState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.FollowWaypoingPathAsTeam, new FollowWaypointsState(this, true), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.FollowWaypointPathAsIndividuals, new FollowWaypointsState(this, false), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.FollowWaypointPathAsTeamExact, new FollowWaypointsExactState(this, true), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.FollowWaypointPathAsIndividualsExact, new FollowWaypointsExactState(this, false), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.FollowPath, new AIState6(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.FollowExitProductionPath, new FollowPathState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.AttackPosition, new AttackState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.AttackObject, new AttackState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.ForceAttackObject, new AttackState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.Dead, new DeadState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.Dock, new DockState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.Enter, new EnterContainerState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.Guard, new GuardState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.Hunt, new HuntState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.Wander, new WanderState(this), AIStateIds.Idle, AIStateIds.MoveAwayFromRepulsors);
        DefineState(AIStateIds.Panic, new PanicState(this), AIStateIds.Idle, AIStateIds.MoveAwayFromRepulsors);
        DefineState(AIStateIds.AttackSquad, new AttackTeamState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.GuardTunnelNetwork, new GuardInTunnelNetworkState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.MoveOutOfTheWay, new AIState23(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.AttackArea, new AttackAreaState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.AttackMoveTo, new AttackMoveState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.AttackFollowWaypointPathAsTeam, new AIState32(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.FaceObject, new FaceState(this, FaceTargetType.FaceNamed), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.FacePosition, new FaceState(this, FaceTargetType.FaceWaypoint), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.RappelInto, new RappelState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.Exit, new ExitContainerState(this), AIStateIds.Idle, AIStateIds.Idle);
        DefineState(AIStateIds.WanderInPlace, new WanderInPlaceState(this), AIStateIds.MoveAwayFromRepulsors, AIStateIds.MoveAwayFromRepulsors);
        DefineState(AIStateIds.Busy, new DoNothingState(this), AIStateIds.Idle, AIStateIds.Idle);
    }

    internal override StateReturnType Update()
    {
        // TODO(Port): Debug logging.

        if (_temporaryState != null)
        {
            // Execute this state.
            var status = _temporaryState.Update();

            if (_temporaryStateFrameEnd < GameEngine.GameLogic.CurrentFrame)
            {
                // Ran out of time.
                if (status.Kind == StateReturnTypeKind.Continue)
                {
                    status = StateReturnType.Success;
                }
            }
            if (status.Kind == StateReturnTypeKind.Continue)
            {
                // TODO(Port): Debug logging.
                return status;
            }
            _temporaryState.OnExit(StateExitType.Normal);
            _temporaryState = null;
        }

        // TODO(Port): Debug logging.

        return base.Update();
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

        var temporaryStateId = StateId.Invalid;
        if (reader.Mode == StatePersistMode.Write && _temporaryState != null)
        {
            temporaryStateId = _temporaryState.Id;
            DebugUtility.AssertCrash(temporaryStateId != StateId.Invalid, "State has invalid state id");
        }
        reader.PersistStateId(ref temporaryStateId);
        if (reader.Mode == StatePersistMode.Read && temporaryStateId != StateId.Invalid)
        {
            _temporaryState = GetState(temporaryStateId);
        }
        if (_temporaryState != null)
        {
            reader.PersistObject(_temporaryState);
        }

        reader.PersistLogicFrame(ref _temporaryStateFrameEnd);
    }
}

internal sealed class TargetTeam : IPersistableObject
{
    private readonly List<ObjectId> _objectIds = new();

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistList(
            _objectIds,
            static (StatePersister persister, ref ObjectId item) =>
        {
            persister.PersistObjectIdValue(ref item);
        });
    }
}

public static class AIStateIds
{
    public static readonly StateId Idle = new(0);
    public static readonly StateId MoveTo = new(1);
    public static readonly StateId FollowWaypoingPathAsTeam = new(2);
    public static readonly StateId FollowWaypointPathAsIndividuals = new(3);
    public static readonly StateId FollowWaypointPathAsTeamExact = new(4);
    public static readonly StateId FollowWaypointPathAsIndividualsExact = new(5);
    public static readonly StateId FollowPath = new(6);
    public static readonly StateId FollowExitProductionPath = new(7);
    public static readonly StateId Wait = new(8);
    public static readonly StateId AttackPosition = new(9);
    public static readonly StateId AttackObject = new(10);
    public static readonly StateId ForceAttackObject = new(11);
    public static readonly StateId AttackAndFollowObject = new(12);
    public static readonly StateId Dead = new(13);
    public static readonly StateId Dock = new(14);
    public static readonly StateId Enter = new(15);
    public static readonly StateId Guard = new(16);
    public static readonly StateId Hunt = new(17);
    public static readonly StateId Wander = new(18);
    public static readonly StateId Panic = new(19);
    public static readonly StateId AttackSquad = new(20);
    public static readonly StateId GuardTunnelNetwork = new(21);
    public static readonly StateId GetRepaired = new(22);
    public static readonly StateId MoveOutOfTheWay = new(23);
    public static readonly StateId MoveAndTighten = new(24);
    public static readonly StateId MoveAndEvacuate = new(25);
    public static readonly StateId MoveAndEvacuateAndExit = new(26);
    public static readonly StateId MoveAndDelete = new(27);
    public static readonly StateId AttackArea = new(28);
    public static readonly StateId HackInternet = new(29);
    public static readonly StateId AttackMoveTo = new(30);
    public static readonly StateId AttackFollowWaypointPathAsIndividuals = new(31);
    public static readonly StateId AttackFollowWaypointPathAsTeam = new(32);
    public static readonly StateId FaceObject = new(33);
    public static readonly StateId FacePosition = new(34);
    public static readonly StateId RappelInto = new(35);
    public static readonly StateId CombatDrop = new(36);
    public static readonly StateId Exit = new(37);
    public static readonly StateId PickUpCrate = new(38);
    public static readonly StateId MoveAwayFromRepulsors = new(39);
    public static readonly StateId WanderInPlace = new(40);
    public static readonly StateId Busy = new(41);
    public static readonly StateId ExitInstantly = new(42);
    public static readonly StateId GuardRetaliate = new(43);
}
