using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class JetAIUpdate : AIUpdate
    {
        public GameObject Base;

        private readonly JetAIUpdateModuleData _moduleData;
        private Vector3 _currentTargetPoint;

        private Queue<string> _pathToStart;
        private Queue<string> _pathToParking;
        private string? _currentUnparkTarget;

        public JetAIState CurrentJetAIState;

        private TimeSpan _waitUntil;

        public enum JetAIState
        {
            REACHED_PARKING_PLACE,
            PARKED,
            UNPARKING_REQUESTED,
            MOVING_TOWARDS_START,
            WAITING_TO_START,
            STARTING,
            STARTED,
            UNPARKED,
            MOVING_TOWARDS_TARGET,
            IDLE,
            RETURNING_TO_BASE,
            LANDING,
            MOVING_BACK_TO_PARKING
        }

        internal JetAIUpdate(GameObject gameObject, JetAIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
            CurrentJetAIState = JetAIState.UNPARKED;
        }

        internal override void SetTargetPoint(Vector3 targetPoint)
        {
            switch(CurrentJetAIState)
            {
                case JetAIState.PARKED:
                    CurrentJetAIState = JetAIState.UNPARKING_REQUESTED;
                    _currentTargetPoint = targetPoint;
                    return;
                case JetAIState.UNPARKING_REQUESTED:
                case JetAIState.MOVING_TOWARDS_START:
                case JetAIState.STARTING:
                    _currentTargetPoint = targetPoint;
                    return;
                case JetAIState.STARTED:
                case JetAIState.IDLE:
                case JetAIState.RETURNING_TO_BASE:
                    CurrentJetAIState = JetAIState.MOVING_TOWARDS_TARGET;
                    break;
                case JetAIState.MOVING_BACK_TO_PARKING:
                    // TODO: check vanilla behavior
                    return;
            }
            base.SetTargetPoint(targetPoint);
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);

            var parkingPlaceBehavior = Base.FindBehavior<ParkingPlaceBehaviour>();

            var isMoving = GameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving);

            switch (CurrentJetAIState)
            {
                case JetAIState.REACHED_PARKING_PLACE:
                    var creationPoint = Base.ToWorldspace(parkingPlaceBehavior.GetUnitCreateTransform(GameObject)).Translation;
                    var parkingPoint = Base.ToWorldspace(parkingPlaceBehavior.GetParkingTransform(GameObject)).Translation;
                    SetTargetDirection(parkingPoint - creationPoint);
                    CurrentJetAIState = JetAIState.PARKED;
                    break;
                case JetAIState.PARKED:
                    break;
                case JetAIState.UNPARKING_REQUESTED:
                    _pathToStart = parkingPlaceBehavior.GetPathToStart(GameObject);
                    CurrentJetAIState = JetAIState.MOVING_TOWARDS_START;
                    break;
                case JetAIState.MOVING_TOWARDS_START:
                    if (!isMoving)
                    {
                        if (ProcessWaypointPath(parkingPlaceBehavior, _pathToStart))
                        {
                            break;
                        }
                        CurrentJetAIState = JetAIState.WAITING_TO_START;
                        _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.TakeoffPause);
                    }
                    break;
                case JetAIState.WAITING_TO_START:
                    if (context.Time.TotalTime > _waitUntil)
                    {
                        SetLocomotor(LocomotorSetType.Normal);
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetExhaust, true);
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetAfterburner, true);
                        base.SetTargetPoint(Base.ToWorldspace(parkingPlaceBehavior.GetRunwayEndPoint(GameObject)));
                        CurrentJetAIState = JetAIState.STARTING;
                    }
                    break;
                case JetAIState.STARTING:
                    if (!isMoving)
                    {
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetAfterburner, false);
                        base.SetTargetPoint(_currentTargetPoint);
                        CurrentJetAIState = JetAIState.MOVING_TOWARDS_TARGET;
                    }
                    break;
                case JetAIState.MOVING_TOWARDS_TARGET:
                    if (!isMoving)
                    {
                        CurrentJetAIState = JetAIState.IDLE;
                        _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.ReturnToBaseIdleTime);
                    }
                    break;
                case JetAIState.IDLE:
                    if (context.Time.TotalTime > _waitUntil)
                    {
                        base.SetTargetPoint(Base.ToWorldspace(parkingPlaceBehavior.GetRunwayEndPoint(GameObject)));
                        CurrentJetAIState = JetAIState.RETURNING_TO_BASE;
                    }
                    break;
                case JetAIState.RETURNING_TO_BASE:
                    if (!isMoving)
                    {
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetExhaust, false);
                        CurrentJetAIState = JetAIState.LANDING;
                        SetLocomotor(LocomotorSetType.Taxiing);
                        _pathToParking = parkingPlaceBehavior.GetPathToParking(GameObject);
                    }
                    break;
                case JetAIState.LANDING:
                    if (!isMoving)
                    {
                        if (ProcessWaypointPath(parkingPlaceBehavior, _pathToParking))
                        {
                            break;
                        }
                        CurrentJetAIState = JetAIState.REACHED_PARKING_PLACE;
                    }
                    break;
            }

            var transform = GameObject.Transform;
            var trans = transform.Translation;

            var terrainHeight = context.GameContext.Terrain.HeightMap.GetHeight(trans.X, trans.Y);
            if (trans.Z - terrainHeight < _moduleData.MinHeight)
            {
                trans.Z = terrainHeight + _moduleData.MinHeight;
                transform.Translation = trans;
            }
        }

        private bool ProcessWaypointPath(ParkingPlaceBehaviour parkingPlaceBehavior, Queue<string> path)
        {
            if (_currentUnparkTarget != null)
            {
                parkingPlaceBehavior.SetPointBlocked(_currentUnparkTarget, false);
            }

            if (path.Count > 0)
            {
                var nextPoint = path.Peek();
                if (parkingPlaceBehavior.IsPointBlocked(nextPoint))
                {
                    return true;
                }
                _currentUnparkTarget = nextPoint;
                parkingPlaceBehavior.SetPointBlocked(nextPoint, true);
                base.SetTargetPoint(Base.ToWorldspace(parkingPlaceBehavior.GetBoneTranslation(path.Dequeue())));
                return true;
            }

            return false;
        }
    }


    /// <summary>
    /// Allows the use of VoiceLowFuel and Afterburner within UnitSpecificSounds section of the object.
    /// Requires Kindof = AIRCRAFT.
    /// Allows the use of JETEXHAUST JETAFTERBURNER model condition states; this is triggered when
    /// it's taking off from the runway.
    /// </summary>
    public sealed class JetAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static JetAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<JetAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<JetAIUpdateModuleData>
            {
                { "OutOfAmmoDamagePerSecond", (parser, x) => x.OutOfAmmoDamagePerSecond = parser.ParsePercentage() },
                { "TakeoffSpeedForMaxLift", (parser, x) => x.TakeoffSpeedForMaxLift = parser.ParsePercentage() },
                { "TakeoffDistForMaxLift", (parser, x) => x.TakeoffDistForMaxLift = parser.ParsePercentage() },
                { "TakeoffPause", (parser, x) => x.TakeoffPause = parser.ParseInteger() },
                { "MinHeight", (parser, x) => x.MinHeight = parser.ParseInteger() },
                { "NeedsRunway", (parser, x) => x.NeedsRunway = parser.ParseBoolean() },
                { "KeepsParkingSpaceWhenAirborne", (parser, x) => x.KeepsParkingSpaceWhenAirborne = parser.ParseBoolean() },
                { "SneakyOffsetWhenAttacking", (parser, x) => x.SneakyOffsetWhenAttacking = parser.ParseFloat() },
                { "AttackLocomotorType", (parser, x) => x.AttackLocomotorType = parser.ParseEnum<LocomotorSetType>() },
                { "AttackLocomotorPersistTime", (parser, x) => x.AttackLocomotorPersistTime = parser.ParseInteger() },
                { "AttackersMissPersistTime", (parser, x) => x.AttackersMissPersistTime = parser.ParseInteger() },
                { "ReturnForAmmoLocomotorType", (parser, x) => x.ReturnForAmmoLocomotorType = parser.ParseEnum<LocomotorSetType>() },
                { "ParkingOffset", (parser, x) => x.ParkingOffset = parser.ParseInteger() },
                { "ReturnToBaseIdleTime", (parser, x) => x.ReturnToBaseIdleTime = parser.ParseInteger() },
            });

        /// <summary>
        /// Amount of damage, as a percentage of max health, to take per second when out of ammo.
        /// </summary>
        public Percentage OutOfAmmoDamagePerSecond { get; private set; }
        /// <summary>
        /// smaller numbers give more lift sooner when taking off
        /// </summary>
        public Percentage TakeoffSpeedForMaxLift { get; private set; }
        public int TakeoffPause { get; private set; }
        public int MinHeight { get; private set; }
        /// <summary>
        /// comanche (helicopter) does not need a runway
        /// </summary>
        public bool NeedsRunway { get; private set; }
        /// <summary>
        /// comanche (helicopter) does not keep its parking space
        /// </summary>
        public bool KeepsParkingSpaceWhenAirborne { get; private set; }
        /// <summary>
        /// this is how far behind us people aim when we are in attack mode
        /// </summary>
        public float SneakyOffsetWhenAttacking { get; private set; }
        public LocomotorSetType AttackLocomotorType { get; private set; }
        /// <summary>
        /// we start slowing down almost immediately
        /// </summary>
        public int AttackLocomotorPersistTime { get; private set; }
        /// <summary>
        /// but remain untargetable fer a bit longer
        /// </summary>
        public int AttackersMissPersistTime { get; private set; }
        public LocomotorSetType ReturnForAmmoLocomotorType { get; private set; }
        /// <summary>
        /// scooch it a little forward so the tail doesn't hit the doors
        /// </summary>
        public int ParkingOffset { get; private set; }
        /// <summary>
        /// if idle for this long, return to base, even if not out of ammo
        /// </summary>
        public int ReturnToBaseIdleTime { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public Percentage TakeoffDistForMaxLift { get; private set; }

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new JetAIUpdate(gameObject, this);
        }
    }
}
