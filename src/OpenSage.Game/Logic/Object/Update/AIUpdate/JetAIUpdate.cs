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

        private bool _afterburnerEnabled;

        public enum JetAIState
        {
            MovingOut,
            ReachedParkingPlace,
            Rotating, // better name
            ScoochingForward,
            Parked,
            UnparkingRequested,
            MovingTowardsStart,
            WaitingToStart,
            Starting,
            Started,
            MovingTowardsTarget,
            Idle,
            ReturningToBase,
            Landing,
            MovingBackToParking
        }

        internal JetAIUpdate(GameObject gameObject, JetAIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
            CurrentJetAIState = JetAIState.Parked;
        }

        internal override void SetTargetPoint(Vector3 targetPoint)
        {
            switch(CurrentJetAIState)
            {
                // TODO:
                case JetAIState.Parked:
                    CurrentJetAIState = JetAIState.UnparkingRequested;
                    _currentTargetPoint = targetPoint;
                    return;
                case JetAIState.UnparkingRequested:
                case JetAIState.MovingTowardsStart:
                case JetAIState.Starting:
                    _currentTargetPoint = targetPoint;
                    return;
                case JetAIState.Started:
                case JetAIState.Idle:
                case JetAIState.ReturningToBase:
                    CurrentJetAIState = JetAIState.MovingTowardsTarget;
                    break;
                case JetAIState.MovingBackToParking:
                    // TODO: check vanilla behavior
                    return;
            }
            base.SetTargetPoint(targetPoint);
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);

            if (!_moduleData.KeepsParkingSpaceWhenAirborne) return; // helicopters are way more simple

            var parkingPlaceBehavior = Base.FindBehavior<ParkingPlaceBehaviour>();

            var isMoving = GameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving);

            var transform = GameObject.Transform;
            var trans = transform.Translation;

            var terrainHeight = context.GameContext.Terrain.HeightMap.GetHeight(trans.X, trans.Y);

            switch (CurrentJetAIState)
            {
                case JetAIState.MovingOut:
                    if (isMoving) break;
                    CurrentJetAIState = JetAIState.ReachedParkingPlace;
                    break;

                case JetAIState.ReachedParkingPlace:
                    var createTransform = Base.ToWorldspace(parkingPlaceBehavior.GetUnitCreateTransform(GameObject));
                    SetTargetDirection(createTransform.LookDirection);
                    CurrentJetAIState = JetAIState.Rotating;
                    break;

                case JetAIState.Rotating:
                    if (isMoving) break;
                    base.SetTargetPoint(GameObject.Transform.Translation + GameObject.Transform.LookDirection * _moduleData.ParkingOffset);
                    CurrentJetAIState = JetAIState.ScoochingForward;
                    break;

                case JetAIState.ScoochingForward:
                    if (isMoving) break;
                    CurrentJetAIState = JetAIState.Parked;
                    break;

                case JetAIState.Parked:
                    break;

                case JetAIState.UnparkingRequested:
                    _pathToStart = parkingPlaceBehavior.GetPathToStart(GameObject);
                    CurrentJetAIState = JetAIState.MovingTowardsStart;
                    break;

                case JetAIState.MovingTowardsStart:
                    if (isMoving || ProcessWaypointPath(context, parkingPlaceBehavior, _pathToStart)) break;
                    CurrentJetAIState = JetAIState.WaitingToStart;
                    _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.TakeoffPause);
                    break;

                case JetAIState.WaitingToStart:
                    if (context.Time.TotalTime < _waitUntil) break;
                    SetLocomotor(LocomotorSetType.Normal);
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetExhaust, true);
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetAfterburner, true);
                    _afterburnerEnabled = true;
                    var endPointPosition =
                        Base.ToWorldspace(parkingPlaceBehavior.GetRunwayEndPoint(GameObject));
                    base.SetTargetPoint(endPointPosition);
                    AddTargetPoint(_currentTargetPoint);
                    CurrentJetAIState = JetAIState.Starting;
                    _currentLocomotor.LiftFactor = 0;
                    break;

                case JetAIState.Starting:
                    var speedPercentage = GameObject.Speed / _currentLocomotor.GetSpeed();
                    _currentLocomotor.LiftFactor = speedPercentage;

                    if (speedPercentage < _moduleData.TakeoffSpeedForMaxLift.Value) break;

                    CurrentJetAIState = JetAIState.MovingTowardsTarget;
                    break;

                case JetAIState.MovingTowardsTarget:
                    if (isMoving)
                    {
                        if (_afterburnerEnabled && trans.Z - terrainHeight >= _currentLocomotor.GetLocomotorValue(_ => _.PreferredHeight))
                        {
                            GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetAfterburner, false);
                            _afterburnerEnabled = false;
                        }
                        break;
                    }
                    CurrentJetAIState = JetAIState.Idle;
                    _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.ReturnToBaseIdleTime);
                    break;

                case JetAIState.Idle:
                    if (context.Time.TotalTime < _waitUntil) break;
                    base.SetTargetPoint(Base.ToWorldspace(parkingPlaceBehavior.GetRunwayEndPoint(GameObject)));
                    CurrentJetAIState = JetAIState.ReturningToBase;
                    break;

                case JetAIState.ReturningToBase:
                    if (isMoving) break;
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetExhaust, false);
                    CurrentJetAIState = JetAIState.Landing;
                    SetLocomotor(LocomotorSetType.Taxiing);
                    _pathToParking = parkingPlaceBehavior.GetPathToParking(GameObject);
                    break;

                case JetAIState.Landing:
                    if (isMoving || ProcessWaypointPath(context, parkingPlaceBehavior, _pathToParking)) break;
                    CurrentJetAIState = JetAIState.ReachedParkingPlace;
                    break;
            }

            if (trans.Z - terrainHeight < _moduleData.MinHeight)
            {
                trans.Z = terrainHeight + _moduleData.MinHeight;
                transform.Translation = trans;
            }

            if (GameObject.ModelConditionFlags.Get(ModelConditionFlag.Dying))
            {
                parkingPlaceBehavior.ClearObjectFromSlot(GameObject);
                Base.ProductionUpdate?.CloseDoor(context.Time, parkingPlaceBehavior.GetCorrespondingSlot(GameObject));
            }
        }

        private bool ProcessWaypointPath(BehaviorUpdateContext context, ParkingPlaceBehaviour parkingPlaceBehavior, Queue<string> path)
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
                    _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.TakeoffPause);
                    return true;
                }
                if (context.Time.TotalTime < _waitUntil) return true;

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
        public bool NeedsRunway { get; private set; } = true;

        /// <summary>
        /// comanche (helicopter) does not keep its parking space
        /// </summary>
        public bool KeepsParkingSpaceWhenAirborne { get; private set; } = true;
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
