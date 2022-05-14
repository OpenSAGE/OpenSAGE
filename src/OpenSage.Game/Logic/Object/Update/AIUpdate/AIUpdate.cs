using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Logic.AI;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class AIUpdate : UpdateModule
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly AIStateMachine _stateMachine;

        private readonly LocomotorSet _locomotorSet;
        private LocomotorSetType _currentLocomotorSetType;

        public Locomotor CurrentLocomotor { get; protected set; }

        private readonly AIUpdateModuleData _moduleData;

        protected readonly GameObject GameObject;

        private readonly TurretAIUpdate _turretAIUpdate;

        /// <summary>
        /// An enumerator of the waypoints if the unit is currently following a waypoint path.
        /// The path (as in pathfinding) to the next waypoint can contain multiple <see cref="TargetPoints"/>.
        /// </summary>
        private IEnumerator<Vector3> _waypointEnumerator;

        private Vector3? _targetDirection;

        private uint _unknownInt1;
        private uint _unknownInt2;
        private uint _unknownInt3;
        private bool _unknownBool1;
        private bool _unknownBool2;
        private uint _unknownInt4;
        private uint _unknownInt5;
        private float _unknownFloat1;
        private uint _unknownInt6;
        private uint _unknownInt7;
        private uint _unknownInt8;
        private uint _unknownInt9;
        private uint _unknownInt10;
        private uint _unknownInt11;
        private uint _unknownInt12;
        private uint _unknownInt13;
        private bool _unknownBool3;
        private bool _unknownBool4;
        private bool _unknownBool5;
        private bool _unknownBool6;
        private string _guardAreaPolygonTriggerName;
        private string _attackPriorityName;
        private uint _unknownInt14;
        private bool _unknownBool7;
        private uint _unknownInt15;
        private bool _unknownBool8;
        private PathfindingPath _path;
        private uint _unknownInt16;
        private Vector3 _unknownPosition1;
        private uint _unknownObjectId;
        private float _unknownFloat2;
        private Point2D _unknownPos2D1;
        private Point2D _unknownPos2D2;
        private uint _unknownFrame1;
        private uint _unknownFrame2;
        private Vector3 _unknownPosition2;
        private bool _unknownBool9;
        private bool _unknownBool10;
        private bool _unknownBool11;
        private bool _unknownBool12;
        private uint _unknownObjectId2;
        private uint _unknownInt17;
        private uint _unknownInt18;
        private float _angleSomething;
        private int _unknownInt19;
        private int _unknownInt20;
        private uint _unknownFrame3;

        /// <summary>
        /// A list of positions along the path to the current target point. "Path" as in pathfinding, not waypoint path.
        /// </summary>
        public List<Vector3> TargetPoints { get; set; }

        internal AIUpdate(GameObject gameObject, AIUpdateModuleData moduleData)
        {
            GameObject = gameObject;
            _moduleData = moduleData;

            TargetPoints = new List<Vector3>();

            _stateMachine = new AIStateMachine();

            _locomotorSet = new LocomotorSet(gameObject);
            _currentLocomotorSetType = (LocomotorSetType)(-1);

            SetLocomotor(LocomotorSetType.Normal);

            if (_moduleData.Turret != null)
            {
                _turretAIUpdate = _moduleData.Turret.CreateTurretAIUpdate(GameObject);
            }
        }

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

            if (!GameObject.Definition.KindOf.Get(ObjectKinds.Aircraft))
            {
                var start = GameObject.Translation;
                var path = GameObject.GameContext.Navigation.CalculatePath(start, targetPoint, out var endIsPassable);
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

        internal void Stop()
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
                Stop();
            }
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (_turretAIUpdate != null)
            {
                _turretAIUpdate.Update(context, _moduleData.AutoAcquireEnemiesWhenIdle);
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
                context.GameContext.Quadtree.Update(GameObject);
            }

            if (CurrentLocomotor == null)
            {
                return;
            }

            if (TargetPoints.Count > 0)
            {
                Vector3? nextPoint = null;

                if (TargetPoints.Count > 1)
                {
                    nextPoint = TargetPoints[1];
                }

                var reachedPosition = CurrentLocomotor.MoveTowardsPosition(TargetPoints[0], context.GameContext.Terrain.HeightMap, nextPoint);

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
                if (!CurrentLocomotor.RotateToTargetDirection(_targetDirection.Value))
                {
                    return;
                }

                _targetDirection = null;
                Stop();
            }
            else
            {
                // maintain position (jets etc)
                CurrentLocomotor.MaintainPosition(context.GameContext.Terrain.HeightMap);
            }
        }

        internal override void DrawInspector()
        {
            // TODO: Locomotor?
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(4);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistUInt32(ref _unknownInt1);
            reader.PersistUInt32(ref _unknownInt2);
            reader.PersistObject(_stateMachine);
            reader.PersistUInt32(ref _unknownInt3);
            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);
            reader.PersistUInt32(ref _unknownInt4);
            reader.PersistUInt32(ref _unknownInt5);
            reader.PersistSingle(ref _unknownFloat1); // 999999
            reader.PersistUInt32(ref _unknownInt6); // 2
            reader.PersistUInt32(ref _unknownInt7); // 3
            reader.PersistUInt32(ref _unknownInt8); // 3
            reader.PersistUInt32(ref _unknownInt9); // 3
            reader.PersistUInt32(ref _unknownInt10); // 0
            reader.PersistUInt32(ref _unknownInt11); // 0
            reader.PersistUInt32(ref _unknownInt12); // 0
            reader.PersistUInt32(ref _unknownInt13); // 0
            reader.PersistBoolean(ref _unknownBool3);
            reader.PersistBoolean(ref _unknownBool4);
            reader.PersistBoolean(ref _unknownBool5); // 0
            reader.PersistBoolean(ref _unknownBool6); // 0
            reader.PersistAsciiString(ref _guardAreaPolygonTriggerName);
            reader.PersistAsciiString(ref _attackPriorityName);
            reader.PersistUInt32(ref _unknownInt14); // 0
            reader.PersistBoolean(ref _unknownBool7);
            reader.PersistUInt32(ref _unknownInt15); // 0

            var unknownInt15_1 = 0x7FFFFFFFu;
            reader.PersistUInt32(ref unknownInt15_1);
            if (unknownInt15_1 != 0x7FFFFFFF)
            {
                throw new InvalidStateException();
            }

            reader.PersistBoolean(ref _unknownBool8);

            var hasPath = _path != null;
            reader.PersistBoolean(ref hasPath);
            if (hasPath)
            {
                _path ??= new PathfindingPath();
                reader.PersistObject(_path);
            }

            reader.PersistUInt32(ref _unknownInt16);
            reader.PersistVector3(ref _unknownPosition1);

            reader.SkipUnknownBytes(12);

            reader.PersistObjectID(ref _unknownObjectId);
            reader.PersistSingle(ref _unknownFloat2);
            reader.PersistPoint2D(ref _unknownPos2D1);
            reader.PersistPoint2D(ref _unknownPos2D2);
            reader.PersistFrame(ref _unknownFrame1);
            reader.PersistFrame(ref _unknownFrame2);
            reader.PersistVector3(ref _unknownPosition2);

            reader.SkipUnknownBytes(1);

            reader.PersistBoolean(ref _unknownBool9);
            reader.PersistBoolean(ref _unknownBool10);

            reader.SkipUnknownBytes(5);

            reader.PersistBoolean(ref _unknownBool11);
            reader.PersistBoolean(ref _unknownBool12);

            reader.SkipUnknownBytes(8);

            reader.PersistObjectID(ref _unknownObjectId2);

            reader.SkipUnknownBytes(4);

            reader.PersistObject(_locomotorSet);

            var currentLocomotorTemplateName = CurrentLocomotor?.LocomotorTemplate.Name;
            reader.PersistAsciiString(ref currentLocomotorTemplateName);

            if (reader.Mode == StatePersistMode.Read)
            {
                CurrentLocomotor = currentLocomotorTemplateName != ""
                    ? _locomotorSet.GetLocomotor(currentLocomotorTemplateName)
                    : null;
            }

            reader.PersistUInt32(ref _unknownInt17);
            if (_unknownInt17 != 0 && _unknownInt17 != 3 && _unknownInt17 != uint.MaxValue)
            {
                throw new InvalidStateException();
            }

            reader.PersistUInt32(ref _unknownInt18); // 0, 1
            reader.PersistSingle(ref _angleSomething);

            reader.SkipUnknownBytes(8);

            if (_moduleData.Turret != null)
            {
                reader.PersistObject(_turretAIUpdate, "TurretAI");
            }

            reader.PersistInt32(ref _unknownInt19); // -1, 258

            reader.PersistInt32(ref _unknownInt20);
            if (_unknownInt20 != 0 && _unknownInt20 != 1 && _unknownInt20 != 2 && _unknownInt20 != -2)
            {
                throw new InvalidStateException();
            }

            reader.PersistFrame(ref _unknownFrame3);

            reader.SkipUnknownBytes(4);
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
        public int TimeToEjectPassengersOnRampage  { get; private set; }

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

        internal sealed override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            throw new InvalidOperationException();
        }

        internal virtual AIUpdate CreateAIUpdate(GameObject gameObject) => new AIUpdate(gameObject, this);
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
        private uint _unknownInt1;
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
            reader.PersistUInt32(ref _unknownInt1);
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
}
