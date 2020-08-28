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

        private readonly Dictionary<LocomotorSetType, Locomotor> _locomotors;
        public Locomotor CurrentLocomotor { get; protected set; }

        private readonly AIUpdateModuleData _moduleData;

        protected readonly GameObject GameObject;

        private readonly BehaviorModule _turretAIUpdate;

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

            _locomotors = new Dictionary<LocomotorSetType, Locomotor>();

            SetLocomotor(LocomotorSetType.Normal);

            if (_moduleData.Turret != null)
            {
                _turretAIUpdate = _moduleData.Turret.CreateTurretAIUpdate(GameObject);
            }
        }

        internal void SetLocomotor(LocomotorSetType type)
        {
            if (!_locomotors.TryGetValue(type, out var locomotor))
            {
                var locomotorSet = GameObject.Definition.LocomotorSets.Find(x => x.Condition == type);
                locomotor = (locomotorSet?.Locomotor.Value != null)
                    ? new Locomotor(GameObject, locomotorSet)
                    : null;
                if (locomotor != null)
                {
                    _locomotors[type] = locomotor;
                }
            }

            CurrentLocomotor = locomotor;
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
                _turretAIUpdate.Update(context);
            }
            else
            {
                var target = GameObject.CurrentWeapon?.CurrentTarget;
                if (target != null)
                {
                    var directionToTarget = (target.TargetPosition - GameObject.Transform.Translation).Vector2XY();
                    SetTargetDirection();
                }
            }

            if (GameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving))
            {
                context.GameContext.Quadtree.Update(GameObject);
            }

            if (_currentLocomotor != null && TargetPoints.Count > 0)
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

            var unknown1 = reader.ReadBytes(50);

            var unknown2 = reader.ReadSingle();

            var unknown3 = reader.ReadBytes(16);

            var unknown4 = reader.ReadSingle();

            var unknown5 = reader.ReadUInt32();
            var unknown6 = reader.ReadUInt32();
            var unknown7 = reader.ReadUInt32();
            var unknown8 = reader.ReadUInt32();

            var unknown9 = reader.ReadBytes(31);

            var unknown10 = reader.ReadInt32();

            var unknown11 = reader.ReadBytes(103);

            var locomotorTemplate = reader.ReadBytePrefixedAsciiString();

            var unknown12 = reader.ReadByte();
            var unknown13 = reader.ReadUInt32();

            var position = reader.ReadVector3();

            for (var i = 0; i < 7; i++)
            {
                var unknown14 = reader.ReadSingle();
            }

            var unknown15 = reader.ReadBytes(8);

            for (var i = 0; i < 3; i++)
            {
                var unknown16 = reader.ReadSingle();
            }

            var unknown17 = reader.ReadBytes(5);

            var locomotorTemplate2 = reader.ReadBytePrefixedAsciiString();

            var unknown18 = reader.ReadBytes(36);
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

        internal virtual AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new AIUpdate(gameObject, this);
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
}
