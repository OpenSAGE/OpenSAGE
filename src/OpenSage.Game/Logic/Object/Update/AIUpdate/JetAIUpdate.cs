using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class JetAIUpdate : AIUpdate
    {
        public GameObject Base;

        private readonly JetAIUpdateModuleData _moduleData;
        private Vector3 CurrentTargetPoint;

        public JetAIState CurrentJetAIState;

        public enum JetAIState
        {
            PARKED,
            UNPARKING_REQUESTED,
            MOVING_TOWARDS_PREP,
            MOVING_TOWARDS_START,
            STARTING,
            STARTED,
            UNPARKED,
            MOVING_TOWARDS_TARGET,
            IDLE,
            MOVING_BACK_TO_BASE,
            MOVING_BACK_TO_PREP,
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
                    CurrentTargetPoint = targetPoint;
                    return;
                case JetAIState.UNPARKING_REQUESTED:
                case JetAIState.MOVING_TOWARDS_PREP:
                case JetAIState.MOVING_TOWARDS_START:
                case JetAIState.STARTING:
                    CurrentTargetPoint = targetPoint;
                    return;
                case JetAIState.STARTED:
                case JetAIState.IDLE:
                case JetAIState.MOVING_BACK_TO_BASE:
                    CurrentJetAIState = JetAIState.MOVING_TOWARDS_TARGET;
                    break;
                case JetAIState.MOVING_BACK_TO_PREP:
                case JetAIState.MOVING_BACK_TO_PARKING:
                    // TODO: check vanilla behavior
                    return;
            }
            base.SetTargetPoint(targetPoint);
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            var parkingPlaceBehavior = Base.FindBehavior<ParkingPlaceBehaviour>();

            var isMoving = GameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving);

            switch (CurrentJetAIState)
            {
                case JetAIState.PARKED:
                    break;
                case JetAIState.UNPARKING_REQUESTED:
                    SetTargetPoint(parkingPlaceBehavior.GetPrepPoint(GameObject));
                    CurrentJetAIState = JetAIState.MOVING_TOWARDS_PREP;
                    break;
                case JetAIState.MOVING_TOWARDS_PREP: // TODO: multiple prep points
                    if (!isMoving)
                    {
                        SetTargetPoint(parkingPlaceBehavior.GetRunwayStartPoint(GameObject));
                        CurrentJetAIState = JetAIState.MOVING_TOWARDS_START;
                    }
                    break;
                case JetAIState.STARTED:
                    SetTargetPoint(CurrentTargetPoint);
                    break;
            }


            //var transform = GameObject.Transform;
            //var trans = transform.Translation;

                //var x = trans.X;
                //var y = trans.Y;
                //var z = trans.Z;

                //var terrainHeight = context.GameContext.Terrain.HeightMap.GetHeight(x, y);

                //for (var i = 0; i < TargetPoints.Count; i++)
                //{
                //    var targetPoint = TargetPoints[i];
                //    if ((targetPoint.Z - terrainHeight) < _moduleData.MinHeight)
                //    {
                //        targetPoint.Z = terrainHeight + _moduleData.MinHeight;
                //    }
                //}

            base.Update(context);
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
        internal static new JetAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<JetAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
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
