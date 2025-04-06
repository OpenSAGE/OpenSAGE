using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Logic.AI;
using OpenSage.Logic.Map;
using OpenSage.Mathematics;
using OpenSage.Scripting;

namespace OpenSage.Logic.Object;

public class AIUpdate : UpdateModule
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private AIUpdateStateMachine _stateMachine;

    private protected AIUpdateStateMachine StateMachine => _stateMachine ??= CreateStateMachine();

    private readonly LocomotorSet _locomotorSet;
    private LocomotorSetType _currentLocomotorSetType;

    public LocomotorSetType CurrentLocomotorSetType => _currentLocomotorSetType;

    public Locomotor CurrentLocomotor { get; protected set; }

    internal virtual AIUpdateModuleData ModuleData { get; }

    private readonly TurretAIUpdate _turretAIUpdate;

    /// <summary>
    /// An enumerator of the waypoints if the unit is currently following a waypoint path.
    /// The path (as in pathfinding) to the next waypoint can contain multiple <see cref="TargetPoints"/>.
    /// </summary>
    private IEnumerator<Vector3> _waypointEnumerator;

    private Vector3? _targetDirection;

    private uint _priorWaypointId;
    private uint _currentWaypointId;
    private bool _isAIDead;
    private bool _isRecruitable;
    private uint _nextEnemyScanTime;
    private ObjectId _currentVictimId;
    private float _desiredSpeed;
    private CommandSourceType _lastCommandSource;
    private GuardTargetType _guardTargetType0;
    private GuardTargetType _guardTargetType1;
    private Vector3 _locationToGuard;
    private ObjectId _objectToGuard;
    private PolygonTrigger _areaToGuard;
    private AttackPriority _attackInfo;
    private readonly List<Vector3> _waypointQueue = new(16);
    private int _waypointIndex;
    private bool _executingWaypointQueue;
    private Waypoint _completedWaypoint;
    private bool _waitingForPath;
    private PathfindingPath _path;
    private ObjectId _requestedVictimId;
    private Vector3 _requestedDestination;
    private Vector3 _requestedDestination2;
    private ObjectId _ignoreObstacleId;
    private float _pathExtraDistance;
    private Point2D _pathfindGoalCell;
    private Point2D _pathfindCurrentCell;
    private LogicFrame _ignoreCollisionsUntil;
    private LogicFrame _queueForPathFrame;
    private Vector3 _finalPosition;
    private bool _doFinalPosition;
    private bool _isAttackPath;
    private bool _isFinalGoal;
    private bool _isApproachPath;
    private bool _isSafePath;
    private bool _movementComplete;
    private bool _upgradedLocomotors;
    private bool _canPathThroughUnits;
    private bool _randomlyOffsetMoodCheck;
    private ObjectId _repulsor1;
    private ObjectId _repulsor2;
    private ObjectId _moveOutOfWay1;
    private ObjectId _moveOutOfWay2;
    private LocomotorGoalType _locomotorGoalType;
    private Vector3 _locomotorGoalData;
    private WhichTurretType _turretSyncFlag;
    private AttitudeType _attitude;
    private LogicFrame _nextMoodCheckTime;
    private ObjectId _crateCreated;

    public ObjectId IgnoredObstacleID => _ignoreObstacleId;

    /// <summary>
    /// A list of positions along the path to the current target point. "Path" as in pathfinding, not waypoint path.
    /// </summary>
    public List<Vector3> TargetPoints { get; set; }

    protected override UpdateOrder UpdateOrder => UpdateOrder.Order0;

    // TODO(Port): Implement this.
    public bool IsMoving => false;

    internal AIUpdate(GameObject gameObject, IGameEngine gameEngine, AIUpdateModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        ModuleData = moduleData;

        TargetPoints = new List<Vector3>();

        _locomotorSet = new LocomotorSet(gameObject, gameEngine);
        _currentLocomotorSetType = LocomotorSetType.Invalid;

        SetLocomotor(LocomotorSetType.Normal);

        // We specifically use the moduleData argument and not the property here, because the property is virtual
        // and might be overridden in a derived class, causing it to be unintialised despite the assignment above.
        if (moduleData.Turret != null)
        {
            _turretAIUpdate = moduleData.Turret.CreateTurretAIUpdate(gameObject, gameEngine);
        }
    }

    private protected virtual AIUpdateStateMachine CreateStateMachine() => new(GameObject, GameEngine, this);

    internal void SetLocomotor(LocomotorSetType type)
    {
        if (_currentLocomotorSetType == type)
        {
            return;
        }

        if (!GameObject.Definition.LocomotorSets.TryGetValue(type, out var locomotorSetTemplate))
        {
            return;
        }

        _locomotorSet.Reset();
        _locomotorSet.Initialize(locomotorSetTemplate);

        // TODO: Use actual surface type.
        CurrentLocomotor = _locomotorSet.GetLocomotorForSurfaces(Surfaces.Ground);
    }

    public bool HasLocomotorForSurface(Surfaces surfaceType)
    {
        return _locomotorSet.GetLocomotorForSurfaces(surfaceType) != null;
    }

    internal void AddTargetPoint(in Vector3 targetPoint)
    {
        TargetPoints.Add(targetPoint);

        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Moving, true);
    }

    internal void AppendPathToTargetPoint(in Vector3 targetPoint)
    {
        if (GameObject.Definition.KindOf == null)
        {
            return;
        }

        if (!GameObject.Definition.KindOf.Get(ObjectKinds.Aircraft) && targetPoint != GameObject.Translation)
        {
            var start = GameObject.Translation;
            var path = GameEngine.Navigation.CalculatePath(start, targetPoint, out var endIsPassable);
            if (path.Count > 0)
            {
                path.RemoveAt(0);
            }
            TargetPoints.AddRange(path);
            if (TargetPoints.Count > 0 && endIsPassable)
            {
                TargetPoints[TargetPoints.Count - 1] = targetPoint;
            }
            Logger.Debug("Set new target points: " + TargetPoints.Count);
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Moving, true);
        }
        else
        {
            AddTargetPoint(targetPoint);
        }
    }

    internal virtual void SetTargetPoint(Vector3 targetPoint)
    {
        if (GameObject.ParentHorde != null)
        {
            return;
        }
        else if (GameObject.Definition.KindOf.Get(ObjectKinds.Horde))
        {
            var targetDirection = targetPoint - GameObject.Translation;
            GameObject.FindBehavior<HordeContainBehavior>()?.SetTargetPoints(targetPoint, targetDirection);
        }

        TargetPoints.Clear();
        AppendPathToTargetPoint(targetPoint);
    }

    internal void FollowWaypoints(IEnumerable<Vector3> waypoints)
    {
        TargetPoints.Clear();
        _waypointEnumerator = waypoints.GetEnumerator();
        MoveToNextWaypointOrStop();
    }

    protected virtual void ArrivedAtDestination()
    {
    }

    /// <summary>
    /// Ceases all behavior by the unit, like when the stop button is pressed.
    /// </summary>
    internal virtual void Stop()
    {
        StopMovingOnly();
    }

    /// <summary>
    /// Stops moving, but does not clear any additional state machine behavior that might be running (e.g. construction, supply gathering, etc)
    /// </summary>
    private void StopMovingOnly()
    {
        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Moving, false);
        _waypointEnumerator?.Dispose();
        _waypointEnumerator = null;
        TargetPoints.Clear();
        GameObject.Speed = 0;
    }

    internal void SetTargetDirection(Vector3 targetDirection)
    {
        _targetDirection = targetDirection;
        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Moving, true);
    }

    /// <summary>
    /// If the unit is currently following a waypoint path, set the next waypoint as target, otherwise stop.
    /// </summary>
    /// <remarks>
    /// It might be necessary to select a path randomly (if there are branches), so this method should only
    /// be called from <see cref="LogicTick"/>.</remarks>
    private void MoveToNextWaypointOrStop()
    {
        if (_waypointEnumerator != null && _waypointEnumerator.MoveNext())
        {
            AddTargetPoint(_waypointEnumerator.Current);
        }
        else
        {
            ArrivedAtDestination();
            StopMovingOnly();
        }
    }

    public void AIEvacuateInstantly(bool exposeStealthUnits, CommandSourceType commandSource)
    {
        // TODO(Port): Implement this.
    }

    public void AIIdle(CommandSourceType commandSource)
    {
        // TODO(Port): Implement this.
    }

    public override UpdateSleepTime Update()
    {
        StateMachine.Update();

        if (_turretAIUpdate != null)
        {
            _turretAIUpdate.Update(ModuleData.AutoAcquireEnemiesWhenIdle);
        }
        else
        {
            var target = GameObject.CurrentWeapon?.CurrentTarget;
            if (target != null)
            {
                var directionToTarget = (target.TargetPosition - GameObject.Translation);
                SetTargetDirection(directionToTarget);
            }
        }

        if (GameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving))
        {
            GameEngine.Quadtree.Update(GameObject);
        }

        if (CurrentLocomotor != null)
        {
            if (TargetPoints.Count > 0)
            {
                Vector3? nextPoint = null;

                if (TargetPoints.Count > 1)
                {
                    nextPoint = TargetPoints[1];
                }

                // TODO(Port): Use correct parameter values.
                var blocked = false;
                CurrentLocomotor.LocoUpdateMoveTowardsPosition(
                    GameObject,
                    TargetPoints[0],
                    Vector3.Distance(GameObject.Translation, TargetPoints[0]),
                    CurrentLocomotor.GetMaxSpeedForCondition(GameObject.BodyModule.DamageState),
                    ref blocked);

                // TODO(Port): This isn't right.
                var reachedPosition = (GameObject.Translation - TargetPoints[0]).Vector2XY().LengthSquared() < 0.25f;

                // this should be moved to LogicTick
                if (reachedPosition)
                {
                    Logger.Debug($"Reached point {TargetPoints[0]}");
                    TargetPoints.RemoveAt(0);
                    if (TargetPoints.Count == 0)
                    {
                        MoveToNextWaypointOrStop();
                    }
                }
            }
            else if (_targetDirection.HasValue)
            {
                var targetYaw = MathUtility.GetYawFromDirection(new Vector2(_targetDirection.Value.X, _targetDirection.Value.Y));
                CurrentLocomotor.LocoUpdateMoveTowardsAngle(GameObject, targetYaw);

                // TODO(Port): This isn't right.
                var reachedAngle = MathUtility.CalculateAngleDelta(targetYaw, GameObject.Yaw) < 0.1f;

                if (!reachedAngle)
                {
                    // TODO(Port): Use correct value.
                    return UpdateSleepTime.None;
                }

                _targetDirection = null;
                Stop();
            }
            else
            {
                // maintain position (jets etc)
                CurrentLocomotor.LocoUpdateMaintainCurrentPosition(GameObject);
            }
        }

        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }

    internal override void DrawInspector()
    {
        base.DrawInspector();

        if (ImGui.TreeNodeEx("Target points"))
        {
            for (var i = 0; i < TargetPoints.Count; i++)
            {
                var targetPoint = TargetPoints[i];
                if (ImGui.InputFloat3("Target point", ref targetPoint))
                {
                    TargetPoints[i] = targetPoint;
                }
            }

            ImGui.TreePop();
        }

        ImGui.SeparatorText("Locomotor");

        CurrentLocomotor?.DrawInspector();
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(4);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistUInt32(ref _priorWaypointId);
        reader.PersistUInt32(ref _currentWaypointId);
        reader.PersistObject(StateMachine);
        reader.PersistBoolean(ref _isAIDead);
        reader.PersistBoolean(ref _isRecruitable);
        reader.PersistUInt32(ref _nextEnemyScanTime);
        reader.PersistObjectId(ref _currentVictimId);
        reader.PersistSingle(ref _desiredSpeed);
        reader.PersistEnum(ref _lastCommandSource);
        reader.PersistEnum(ref _guardTargetType0);

        // Original game accidentally used 8 bytes for _guardTargetType0.
        // The extra 4 bytes are actually the same value as _guardTargetType1.
        var garbageBytes0 = 0;
        reader.PersistInt32(ref garbageBytes0);

        reader.PersistEnum(ref _guardTargetType1);

        // Original game accidentally used 8 bytes for _guardTargetType1.
        // These 4 bytes contain whatever happens to be in the 4 bytes of memory
        // following _guardTargetType1... which is the first float value in
        // m_locationToGuard.
        var garbageBytes1 = 0;
        reader.PersistInt32(ref garbageBytes1);

        reader.PersistVector3(ref _locationToGuard);
        reader.PersistObjectId(ref _objectToGuard);

        var triggerName = _areaToGuard?.Name ?? "";
        reader.PersistAsciiString(ref triggerName, "AreaToGuard");
        if (reader.Mode == StatePersistMode.Read && triggerName != "")
        {
            _areaToGuard = GameEngine.Scene3D.MapFile.PolygonTriggers.GetPolygonTriggerByName(triggerName);
        }

        var attackName = _attackInfo?.Name ?? "";
        reader.PersistAsciiString(ref attackName);
        if (reader.Mode == StatePersistMode.Read && attackName != "")
        {
            _attackInfo = GameEngine.Game.Scripting.GetAttackInfo(attackName);
        }

        reader.PersistListWithUInt32Count(
            _waypointQueue,
            static (StatePersister persister, ref Vector3 item) =>
            {
                persister.PersistVector3(ref item);
            });

        reader.PersistInt32(ref _waypointIndex);
        reader.PersistBoolean(ref _executingWaypointQueue);

        var completedWaypointId = (uint)(_completedWaypoint?.ID ?? (int)Waypoint.InvalidId);
        reader.PersistUInt32(ref completedWaypointId);
        if (reader.Mode == StatePersistMode.Read)
        {
            GameEngine.Scene3D.Waypoints.TryGetById((int)completedWaypointId, out _completedWaypoint);
        }

        reader.PersistBoolean(ref _waitingForPath);

        var hasPath = _path != null;
        reader.PersistBoolean(ref hasPath);
        if (hasPath)
        {
            _path ??= new PathfindingPath();
            reader.PersistObject(_path);
        }

        reader.PersistObjectId(ref _requestedVictimId);
        reader.PersistVector3(ref _requestedDestination);
        reader.PersistVector3(ref _requestedDestination2);

        reader.PersistObjectId(ref _ignoreObstacleId);
        reader.PersistSingle(ref _pathExtraDistance);
        reader.PersistPoint2D(ref _pathfindGoalCell);
        reader.PersistPoint2D(ref _pathfindCurrentCell);
        reader.PersistLogicFrame(ref _ignoreCollisionsUntil);
        reader.PersistLogicFrame(ref _queueForPathFrame);

        reader.PersistVector3(ref _finalPosition);
        reader.PersistBoolean(ref _doFinalPosition);

        reader.PersistBoolean(ref _isAttackPath);
        reader.PersistBoolean(ref _isFinalGoal);
        reader.PersistBoolean(ref _isApproachPath);
        reader.PersistBoolean(ref _isSafePath);
        reader.PersistBoolean(ref _movementComplete);
        reader.PersistBoolean(ref _isSafePath); // Yes, an exact copy of two lines above
        reader.PersistBoolean(ref _upgradedLocomotors);
        reader.PersistBoolean(ref _canPathThroughUnits);
        reader.PersistBoolean(ref _randomlyOffsetMoodCheck);

        reader.PersistObjectId(ref _repulsor1);
        reader.PersistObjectId(ref _repulsor2);

        reader.PersistObjectId(ref _moveOutOfWay1);
        reader.PersistObjectId(ref _moveOutOfWay2);

        reader.PersistObject(_locomotorSet);

        var currentLocomotorTemplateName = CurrentLocomotor?.LocomotorTemplate.Name ?? "";
        reader.PersistAsciiString(ref currentLocomotorTemplateName);
        if (reader.Mode == StatePersistMode.Read)
        {
            CurrentLocomotor = currentLocomotorTemplateName != ""
                ? _locomotorSet.GetLocomotor(currentLocomotorTemplateName)
                : null;
        }

        reader.PersistEnum(ref _currentLocomotorSetType);
        reader.PersistEnum(ref _locomotorGoalType);
        reader.PersistVector3(ref _locomotorGoalData);

        if (ModuleData.Turret != null)
        {
            reader.PersistObject(_turretAIUpdate, "TurretAI");
        }

        reader.PersistEnum(ref _turretSyncFlag);
        reader.PersistEnum(ref _attitude);
        reader.PersistLogicFrame(ref _nextMoodCheckTime);
        reader.PersistObjectId(ref _crateCreated);
    }

    private enum LocomotorGoalType
    {
        None = 0,
        PositionOnPath = 1,
        PositionExplicit = 2,
        Angle = 3,
    }

    private enum WhichTurretType
    {
        Invalid = -1,
        Main = 0,
        Alt = 1,
    }
}

public class AIUpdateModuleData : UpdateModuleData
{
    internal static AIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    internal static readonly IniParseTable<AIUpdateModuleData> FieldParseTable = new IniParseTable<AIUpdateModuleData>
    {
        { "Turret", (parser, x) => x.Turret = TurretAIUpdateModuleData.Parse(parser) },
        { "AltTurret", (parser, x) => x.AltTurret = TurretAIUpdateModuleData.Parse(parser) },
        { "TurretsLinked", (parser, x) => x.TurretsLinked = parser.ParseBoolean() },
        { "AutoAcquireEnemiesWhenIdle", (parser, x) => x.AutoAcquireEnemiesWhenIdle = parser.ParseEnumBitArray<AutoAcquireEnemiesType>() },
        { "MoodAttackCheckRate", (parser, x) => x.MoodAttackCheckRate = parser.ParseInteger() },
        { "ForbidPlayerCommands", (parser, x) => x.ForbidPlayerCommands = parser.ParseBoolean() },
        { "AILuaEventsList", (parser, x) => x.AILuaEventsList = parser.ParseString() },
        { "HoldGroundCloseRangeDistance", (parser, x) => x.HoldGroundCloseRangeDistance = parser.ParseInteger() },
        { "MinCowerTime", (parser, x) => x.MinCowerTime = parser.ParseInteger() },
        { "MaxCowerTime", (parser, x) => x.MaxCowerTime = parser.ParseInteger() },
        { "CanAttackWhileContained", (parser, x) => x.CanAttackWhileContained = parser.ParseBoolean() },
        { "RampageTime", (parser, x) => x.RampageTime = parser.ParseInteger() },
        { "TimeToEjectPassengersOnRampage", (parser, x) => x.TimeToEjectPassengersOnRampage = parser.ParseInteger() },
        { "AttackPriority", (parser, x) => x.AttackPriority = parser.ParseString() },
        { "SpecialContactPoints", (parser, x) => x.SpecialContactPoints = parser.ParseEnumBitArray<ContactPointType>() },
        { "FadeOnPortals", (parser, x) => x.FadeOnPortals = parser.ParseBoolean() },
        { "StopChaseDistance", (parser, x) => x.StopChaseDistance = parser.ParseInteger() },
        { "RampageRequiresAflame", (parser, x) => x.RampageRequiresAflame = parser.ParseBoolean() },
        { "MoveForNoOne", (parser, x) => x.MoveForNoOne = parser.ParseBoolean() },
        { "StandGround", (parser, x) => x.StandGround = parser.ParseBoolean() },
        { "BurningDeathTime", (parser, x) => x.BurningDeathTime = parser.ParseInteger() }
    };

    /// <summary>
    /// Allows the use of TurretMoveStart and TurretMoveLoop within the UnitSpecificSounds
    /// section of the object.
    /// </summary>
    public TurretAIUpdateModuleData Turret { get; private set; }

    public TurretAIUpdateModuleData AltTurret { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public bool TurretsLinked { get; private set; }

    public BitArray<AutoAcquireEnemiesType> AutoAcquireEnemiesWhenIdle { get; private set; }
    public int MoodAttackCheckRate { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public bool ForbidPlayerCommands { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public string AILuaEventsList { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int HoldGroundCloseRangeDistance { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int MinCowerTime { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int MaxCowerTime { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public bool CanAttackWhileContained { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int RampageTime { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int TimeToEjectPassengersOnRampage { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public string AttackPriority { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public BitArray<ContactPointType> SpecialContactPoints { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public bool FadeOnPortals { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int StopChaseDistance { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public bool RampageRequiresAflame { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public bool MoveForNoOne { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public bool StandGround { get; private set; }

    [AddedIn(SageGame.Bfme2)]
    public int BurningDeathTime { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new AIUpdate(gameObject, gameEngine, this);
    }
}

public enum AutoAcquireEnemiesType
{
    [IniEnum("YES")]
    Yes,

    [IniEnum("NO")]
    No,

    /// <summary>
    /// Attack buildings in addition to units.
    /// </summary>
    [IniEnum("ATTACK_BUILDINGS")]
    AttackBuildings,

    /// <summary>
    /// Don't counter-attack.
    /// </summary>
    [IniEnum("NotWhileAttacking")]
    NotWhileAttacking,

    [IniEnum("Stealthed")]
    Stealthed,
}

public abstract class BaseAITargetChooserData
{

}

public sealed class UnitAITargetChooserData : BaseAITargetChooserData
{

}

internal sealed class PathfindingPath : IPersistableObject
{
    private readonly List<PathPoint> _points = new();
    private bool _unknownBool1;
    private uint _unknownInt2;
    private bool _unknownBool2;

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistListWithUInt32Count(
            _points,
            static (StatePersister persister, ref PathPoint item) =>
            {
                persister.PersistObjectValue(ref item);
            });

        reader.PersistBoolean(ref _unknownBool1);
        reader.SkipUnknownBytes(4);
        reader.PersistUInt32(ref _unknownInt2); // 1
        reader.PersistBoolean(ref _unknownBool2);
    }
}

internal struct PathPoint : IPersistableObject
{
    private uint _id;
    private Vector3 _position;
    private uint _unknownInt1;
    private bool _unknownBool1;
    private uint _nextId;

    public void Persist(StatePersister reader)
    {
        reader.PersistUInt32(ref _id);
        reader.PersistVector3(ref _position);
        reader.PersistUInt32(ref _unknownInt1);
        reader.PersistBoolean(ref _unknownBool1);
        reader.PersistUInt32(ref _nextId);
    }
}

public enum GuardTargetType
{
    /// <summary>
    /// Guard a location.
    /// </summary>
    Location = 0,

    /// <summary>
    /// Guard an object.
    /// </summary>
    Object = 1,

    /// <summary>
    /// Guard a polygon trigger.
    /// </summary>
    Area = 2,

    /// <summary>
    /// Currently not guarding.
    /// </summary>
    None = 3,
}

public enum AttitudeType
{
    Sleep = -2,
    Passive = -1,
    Normal = 0,
    Alert = 1,
    Aggressive = 2,
    Invalid = 3,
}
