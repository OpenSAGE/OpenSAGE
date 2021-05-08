using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class AIUpdate : UpdateModule
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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

        /// <summary>
        /// A list of positions along the path to the current target point. "Path" as in pathfinding, not waypoint path.
        /// </summary>
        public List<Vector3> TargetPoints { get; set; }

        internal AIUpdate(GameObject gameObject, AIUpdateModuleData moduleData)
        {
            GameObject = gameObject;
            _moduleData = moduleData;

            TargetPoints = new List<Vector3>();

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

                var reachedPosition = CurrentLocomotor.MoveTowardsPosition(context.Time, TargetPoints[0], context.GameContext.Terrain.HeightMap, nextPoint);

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
                if (!CurrentLocomotor.RotateToTargetDirection(context.Time, _targetDirection.Value))
                {
                    return;
                }

                _targetDirection = null;
                Stop();
            }
            else
            {
                // maintain position (jets etc)
                CurrentLocomotor.MaintainPosition(context.Time, context.GameContext.Terrain.HeightMap);
            }
        }

        internal override void DrawInspector()
        {
            // TODO: Locomotor?
        }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 4)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknownInt1 = reader.ReadUInt32();
            var unknownInt2 = reader.ReadUInt32();

            var unknownVersion1 = reader.ReadVersion();
            if (unknownVersion1 != 1)
            {
                throw new InvalidDataException();
            }

            var unknownVersion2 = reader.ReadVersion();
            if (unknownVersion2 != 1)
            {
                throw new InvalidDataException();
            }

            var unknownInt3 = reader.ReadUInt32(); // 0
            var unknownInt4 = reader.ReadUInt32(); // 0

            var unknownInt5 = reader.ReadUInt32(); // 1

            var unknownBool1 = reader.ReadBooleanChecked();
            if (unknownBool1)
            {
                throw new InvalidDataException();
            }

            var unknownVersion3 = reader.ReadVersion();
            if (unknownVersion3 != 1)
            {
                throw new InvalidDataException();
            }

            var positionSomething = reader.ReadVector3();
            var unknownInt6 = reader.ReadUInt32();
            var unknownBool2 = reader.ReadBooleanChecked();
            var positionSomething2 = reader.ReadVector3();
            var unknownInt7 = reader.ReadUInt32();
            var unknownInt8 = reader.ReadUInt32();
            var unknownBool3 = reader.ReadBooleanChecked();
            var unknownInt9 = reader.ReadUInt32();
            var positionSomething3 = reader.ReadVector3();
            var unknownBool4 = reader.ReadBooleanChecked();
            var unknownBool5 = reader.ReadBooleanChecked();

            var unknownInt10 = reader.ReadUInt32();
            if (unknownInt10 != 0)
            {
                throw new InvalidDataException();
            }

            var unknownBool6 = reader.ReadBooleanChecked();
            var unknownBool7 = reader.ReadBooleanChecked();

            var unknownInt11 = reader.ReadUInt32();
            if (unknownInt11 != 999999)
            {
                throw new InvalidDataException();
            }

            var unknownInt12 = reader.ReadUInt32();
            var unknownBool8 = reader.ReadBooleanChecked();
            var unknownBool9 = reader.ReadBooleanChecked();
            var unknownInt13 = reader.ReadUInt32();
            var unknownInt14 = reader.ReadUInt32();

            var unknownFloat1 = reader.ReadSingle();
            if (unknownFloat1 != 999999)
            {
                throw new InvalidDataException();
            }

            var unknownInt15 = reader.ReadUInt32(); // 2
            var unknownInt16 = reader.ReadUInt32(); // 3
            var unknownInt17 = reader.ReadUInt32(); // 3
            var unknownInt17_1 = reader.ReadUInt32(); // 3

            var unknownInt18 = reader.ReadUInt32(); // 0
            var unknownInt19 = reader.ReadUInt32(); // 0
            var unknownInt20 = reader.ReadUInt32(); // 0
            var unknownInt21 = reader.ReadUInt32(); // 0
            var unknownBool10 = reader.ReadBooleanChecked();
            var unknownBool11 = reader.ReadBooleanChecked();
            var unknownInt22 = reader.ReadUInt32(); // 0
            var unknownInt23 = reader.ReadUInt32(); // 0
            var unknownBool12 = reader.ReadBooleanChecked();
            var unknownInt23_1 = reader.ReadUInt32(); // 0

            var unknownInt24 = reader.ReadUInt32();
            if (unknownInt24 != 0x7FFFFFFF)
            {
                throw new InvalidDataException();
            }

            var unknownBool13 = reader.ReadBooleanChecked();
            var unknownBool14 = reader.ReadBooleanChecked();

            var unknownVersion4 = reader.ReadVersion();
            if (unknownVersion4 != 1)
            {
                throw new InvalidDataException();
            }

            var unknownCount1 = reader.ReadUInt32(); // 2
            for (var i = 0; i < unknownCount1; i++)
            {
                var id = reader.ReadUInt32();
                var position2 = reader.ReadVector3();
                var unknown25 = reader.ReadUInt32();
                var unknownBool15 = reader.ReadBooleanChecked();
                var nextId = reader.ReadUInt32();
            }

            var unknownBool16 = reader.ReadBooleanChecked();
            var unknownInt25 = reader.ReadUInt32();
            var unknownInt26 = reader.ReadUInt32(); // 1
            var unknownBool17 = reader.ReadBooleanChecked();
            var unknownInt27 = reader.ReadUInt32();
            var unknownPosition = reader.ReadVector3();
            reader.ReadBytes(5 * 4);

            for (var i = 0; i < 4; i++)
            {
                var unknownInt28 = reader.ReadInt32();
                if (unknownInt28 != -1)
                {
                    throw new InvalidDataException();
                }
            }

            reader.ReadBytes(46);

            _locomotorSet.Load(new Data.Sav.SaveFileReader(reader));

            var currentLocomotorTemplateName = reader.ReadBytePrefixedAsciiString();
            CurrentLocomotor = _locomotorSet.GetLocomotor(currentLocomotorTemplateName);

            var unknownInt29 = reader.ReadUInt32();
            if (unknownInt29 != 0)
            {
                throw new InvalidDataException();
            }

            var unknownInt30 = reader.ReadUInt32();
            if (unknownInt30 != 1)
            {
                throw new InvalidDataException();
            }

            for (var i = 0; i < 3; i++)
            {
                var unknownInt31 = reader.ReadUInt32();
                if (unknownInt31 != 0)
                {
                    throw new InvalidDataException();
                }
            }

            var unknownInt32 = reader.ReadInt32();
            if (unknownInt32 != -1)
            {
                throw new InvalidDataException();
            }

            var unknownInt33 = reader.ReadInt32();
            if (unknownInt33 != 0)
            {
                throw new InvalidDataException();
            }

            var frameSomething = reader.ReadUInt32();

            var unknownInt34 = reader.ReadInt32();
            if (unknownInt34 != 0)
            {
                throw new InvalidDataException();
            }
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
}
