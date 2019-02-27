using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class Locomotor
    {
        internal static void Parse(IniParser parser, IniDataContext context) => parser.ParseBlockContent(
            (x, name) => x.Name = name,
            context.Locomotors,
            FieldParseTable);

        private static readonly IniParseTable<Locomotor> FieldParseTable = new IniParseTable<Locomotor>
        {
            { "Surfaces", (parser, x) => x.Surfaces = parser.ParseEnumBitArray<Surface>() },
            { "Speed", (parser, x) => x.Speed = parser.ParseFloat() },
            { "SpeedDamaged", (parser, x) => x.SpeedDamaged = parser.ParseFloat() },
            { "MinSpeed", (parser, x) => x.MinSpeed = parser.ParsePercentage() },
            { "TurnRate", (parser, x) => x.TurnRate = parser.ParseFloat() },
            { "TurnRateDamaged", (parser, x) => x.TurnRateDamaged = parser.ParseFloat() },
            { "Acceleration", (parser, x) => x.Acceleration = parser.ParseFloat() },
            { "AccelerationDamaged", (parser, x) => x.AccelerationDamaged = parser.ParseFloat() },
            { "Lift", (parser, x) => x.Lift = parser.ParsePercentage() },
            { "LiftDamaged", (parser, x) => x.LiftDamaged = parser.ParsePercentage() },
            { "Braking", (parser, x) => x.Braking = parser.ParseInteger() },
            { "MinTurnSpeed", (parser, x) => x.MinTurnSpeed = parser.ParsePercentage() },
            { "TurnPivotOffset", (parser, x) => x.TurnPivotOffset = parser.ParseFloat() },
            { "AllowAirborneMotiveForce", (parser, x) => x.AllowAirborneMotiveForce = parser.ParseBoolean() },
            { "PreferredHeight", (parser, x) => x.PreferredHeight = parser.ParseFloat() },
            { "PreferredHeightDamping", (parser, x) => x.PreferredHeightDamping = parser.ParseFloat() },
            { "SpeedLimitZ", (parser, x) => x.SpeedLimitZ = parser.ParseInteger() },
            { "ZAxisBehavior", (parser, x) => x.ZAxisBehavior = parser.ParseEnum<LocomotorZAxisBehavior>() },
            { "Appearance", (parser, x) => x.Appearance = parser.ParseEnum<LocomotorAppearance>() },
            { "StickToGround", (parser, x) => x.StickToGround = parser.ParseBoolean() },
            { "GroupMovementPriority", (parser, x) => x.GroupMovementPriority = parser.ParseEnum<GroupMovementPriority>() },
            { "DownhillOnly", (parser, x) => x.DownhillOnly = parser.ParseBoolean() },

            { "MaxThrustAngle", (parser, x) => x.MaxThrustAngle = parser.ParseInteger() },
            { "ThrustRoll", (parser, x) => x.ThrustRoll = parser.ParseFloat() },
            { "ThrustWobbleRate", (parser, x) => x.ThrustWobbleRate = parser.ParseFloat() },
            { "ThrustMinWobble", (parser, x) => x.ThrustMinWobble = parser.ParseFloat() },
            { "ThrustMaxWobble", (parser, x) => x.ThrustMaxWobble = parser.ParseFloat() },
            { "CloseEnoughDist", (parser, x) => x.CloseEnoughDist = parser.ParseFloat() },
            { "CloseEnoughDist3D", (parser, x) => x.CloseEnoughDist3D = parser.ParseBoolean() },

            { "WanderWidthFactor", (parser, x) => x.WanderWidthFactor = parser.ParseFloat() },
            { "WanderLengthFactor", (parser, x) => x.WanderLengthFactor = parser.ParseFloat() },
            { "WanderAboutPointRadius", (parser, x) => x.WanderAboutPointRadius = parser.ParseFloat() },

            { "AccelerationPitchLimit", (parser, x) => x.AccelerationPitchLimit = parser.ParseFloat() },
            { "DecelerationPitchLimit", (parser, x) => x.DecelerationPitchLimit = parser.ParseFloat() },
            { "BounceAmount", (parser, x) => x.BounceAmount = parser.ParseInteger() },
            { "PitchInDirectionOfZVelFactor", (parser, x) => x.PitchInDirectionOfZVelFactor = parser.ParseFloat() },
            { "PitchStiffness", (parser, x) => x.PitchStiffness = parser.ParseFloat() },
            { "RollStiffness", (parser, x) => x.RollStiffness = parser.ParseFloat() },
            { "PitchDamping", (parser, x) => x.PitchDamping = parser.ParseFloat() },
            { "RollDamping", (parser, x) => x.RollDamping = parser.ParseFloat() },
            { "UniformAxialDamping", (parser, x) => x.UniformAxialDamping = parser.ParseFloat() },

            { "ForwardAccelerationPitchFactor", (parser, x) => x.ForwardAccelerationPitchFactor = parser.ParseFloat() },
            { "LateralAccelerationRollFactor", (parser, x) => x.LateralAccelerationRollFactor = parser.ParseFloat() },
            { "ForwardVelocityPitchFactor", (parser, x) => x.ForwardVelocityPitchFactor = parser.ParseFloat() },
            { "LateralVelocityRollFactor", (parser, x) => x.LateralVelocityRollFactor = parser.ParseFloat() },

            { "Apply2DFrictionWhenAirborne", (parser, x) => x.Apply2DFrictionWhenAirborne = parser.ParseBoolean() },
            { "Extra2DFriction", (parser, x) => x.Extra2DFriction = parser.ParseInteger() },
            { "AirborneTargetingHeight", (parser, x) => x.AirborneTargetingHeight = parser.ParseInteger() },
            { "LocomotorWorksWhenDead", (parser, x) => x.LocomotorWorksWhenDead = parser.ParseBoolean() },
            { "CirclingRadius", (parser, x) => x.CirclingRadius = parser.ParseInteger() },

            { "HasSuspension", (parser, x) => x.HasSuspension = parser.ParseBoolean() },
            { "CanMoveBackwards", (parser, x) => x.CanMoveBackwards = parser.ParseBoolean() },
            { "MaximumWheelExtension", (parser, x) => x.MaximumWheelExtension = parser.ParseFloat() },
            { "MaximumWheelCompression", (parser, x) => x.MaximumWheelCompression = parser.ParseFloat() },
            { "FrontWheelTurnAngle", (parser, x) => x.FrontWheelTurnAngle = parser.ParseInteger() },

            { "SlideIntoPlaceTime", (parser, x) => x.SlideIntoPlaceTime = parser.ParseInteger() },

            { "RudderCorrectionDegree", (parser, x) => x.RudderCorrectionDegree = parser.ParseFloat() },
            { "RudderCorrectionRate", (parser, x) => x.RudderCorrectionRate = parser.ParseFloat() },
            { "ElevatorCorrectionDegree", (parser, x) => x.ElevatorCorrectionDegree = parser.ParseFloat() },
            { "ElevatorCorrectionRate", (parser, x) => x.ElevatorCorrectionRate = parser.ParseFloat() },

            { "TurnTime", (parser, x) => x.TurnTime = parser.ParseInteger() },
            { "TurnTimeDamaged", (parser, x) => x.TurnTimeDamaged = parser.ParseInteger() },
            { "SlowTurnRadius", (parser, x) => x.SlowTurnRadius = parser.ParseFloat() },
            { "FastTurnRadius", (parser, x) => x.FastTurnRadius = parser.ParseFloat() },
            { "NonDirtyTransform", (parser, x) => x.NonDirtyTransform = parser.ParseBoolean() },
            { "PreferredAttackHeight", (parser, x) => x.PreferredAttackHeight = parser.ParseInteger() },
            { "AeleronCorrectionDegree", (parser, x) => x.AeleronCorrectionDegree = parser.ParseFloat() },
            { "AeleronCorrectionRate", (parser, x) => x.AeleronCorrectionRate = parser.ParseFloat() },
            { "SwoopStandoffRadius", (parser, x) => x.SwoopStandoffRadius = parser.ParseFloat() },
            { "SwoopStandoffHeight", (parser, x) => x.SwoopStandoffHeight = parser.ParseFloat() },
            { "SwoopTerminalVelocity", (parser, x) => x.SwoopTerminalVelocity = parser.ParseFloat() },
            { "SwoopAccelerationRate", (parser, x) => x.SwoopAccelerationRate = parser.ParseFloat() },
            { "SwoopSpeedTuningFactor", (parser, x) => x.SwoopSpeedTuningFactor = parser.ParseFloat() },
            { "FormationPriority", (parser, x) => x.FormationPriority = parser.ParseEnum<FormationPriority>() },
            { "BackingUpSpeed", (parser, x) => x.BackingUpSpeed = parser.ParsePercentage() },
            { "ChargeAvailable", (parser, x) => x.ChargeAvailable = parser.ParseBoolean() },
            { "ChargeSpeed", (parser, x) => x.ChargeSpeed = parser.ParsePercentage() },
            { "TurnThreshold", (parser, x) => x.TurnThreshold = parser.ParseFloat() },
            { "TurnThresholdHS", (parser, x) => x.TurnThresholdHS = parser.ParseInteger() },
            { "EnableHighSpeedTurnModelconditions", (parser, x) => x.EnableHighSpeedTurnModelconditions = parser.ParseBoolean() },
            { "UseTerrainSmoothing", (parser, x) => x.UseTerrainSmoothing = parser.ParseBoolean() },
            { "WaitForFormation", (parser, x) => x.WaitForFormation = parser.ParseBoolean() },
            { "MaxTurnWithoutReform", (parser, x) => x.MaxTurnWithoutReform = parser.ParseInteger() },
            { "AccDecTrigger", (parser, x) => x.AccDecTrigger = parser.ParseFloat() },
            { "WalkDistance", (parser, x) => x.WalkDistance = parser.ParseInteger() },
            { "MaxOverlappedHeight", (parser, x) => x.MaxOverlappedHeight = parser.ParseInteger() },
            { "CrewPowered", (parser, x) => x.CrewPowered = parser.ParseBoolean() },
            { "BackingUpStopWhenTurning", (parser, x) => x.BackingUpStopWhenTurning = parser.ParseBoolean() },
            { "BackingUpDistanceMin", (parser, x) => x.BackingUpDistanceMin = parser.ParseInteger() },
            { "BackingUpDistanceMax", (parser, x) => x.BackingUpDistanceMax = parser.ParseInteger() },
            { "BackingUpAngle", (parser, x) => x.BackingUpAngle = parser.ParseFloat() },
            { "LookAheadMult", (parser, x) => x.LookAheadMult = parser.ParseFloat() },
            { "ScalesWalls", (parser, x) => x.ScalesWalls = parser.ParseBoolean() },
            { "ChargeIgnoresCondition", (parser, x) => x.ChargeIgnoresCondition = parser.ParseBoolean() },
            { "BurningDeathRadius", (parser, x) => x.BurningDeathRadius = parser.ParseInteger() },
            { "BurningDeathIsCavalry", (parser, x) => x.BurningDeathIsCavalry = parser.ParseBoolean() },
            { "TurnWhileMoving", (parser, x) => x.TurnWhileMoving = parser.ParseBoolean() },
            { "RiverModifier", (parser, x) => x.ForwardVelocityPitchFactor = parser.ParsePercentage() }
        };

        public string Name { get; private set; }

        public BitArray<Surface> Surfaces { get; private set; }
        public float Speed { get; private set; }
        public float SpeedDamaged { get; private set; }
        public float MinSpeed { get; private set; }
        public float TurnRate { get; private set; }
        public float TurnRateDamaged { get; private set; }
        public float Acceleration { get; private set; }
        public float AccelerationDamaged { get; private set; }
        public float Lift { get; private set; }
        public float LiftDamaged { get; private set; }
        public int Braking { get; private set; }
        public float MinTurnSpeed { get; private set; }
        public float TurnPivotOffset { get; private set; }
        public bool AllowAirborneMotiveForce { get; private set; }
        public float PreferredHeight { get; private set; }
        public float PreferredHeightDamping { get; private set; }
        public int SpeedLimitZ { get; private set; }
        public LocomotorZAxisBehavior ZAxisBehavior { get; private set; }
        public LocomotorAppearance Appearance { get; private set; }
        public bool StickToGround { get; private set; }
        public GroupMovementPriority GroupMovementPriority { get; private set; }
        public bool DownhillOnly { get; private set; }

        public int MaxThrustAngle { get; private set; }
        public float ThrustRoll { get; private set; }
        public float ThrustWobbleRate { get; private set; }
        public float ThrustMinWobble { get; private set; }
        public float ThrustMaxWobble { get; private set; }
        public float CloseEnoughDist { get; private set; } = 1;
        public bool CloseEnoughDist3D { get; private set; }

        // These only apply when Appearance = TwoLegs
        public float WanderWidthFactor { get; private set; }
        public float WanderLengthFactor { get; private set; } = 1;
        public float WanderAboutPointRadius { get; private set; }

        public float AccelerationPitchLimit { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float DecelerationPitchLimit { get; private set; }

        public int BounceAmount { get; private set; }
        public float PitchInDirectionOfZVelFactor { get; private set; }
        public float PitchStiffness { get; private set; } = 0.1f;
        public float RollStiffness { get; private set; } = 0.1f;
        public float PitchDamping { get; private set; } = 0.9f;
        public float RollDamping { get; private set; } = 0.9f;
        public float UniformAxialDamping { get; private set; } = 1;

        public float ForwardAccelerationPitchFactor { get; private set; }
        public float LateralAccelerationRollFactor { get; private set; }
        public float ForwardVelocityPitchFactor { get; private set; }
        public float LateralVelocityRollFactor { get; private set; }

        public bool Apply2DFrictionWhenAirborne { get; private set; }
        public int Extra2DFriction { get; private set; }
        public int AirborneTargetingHeight { get; private set; }
        public bool LocomotorWorksWhenDead { get; private set; }
        public int CirclingRadius { get; private set; }

        public bool HasSuspension { get; private set; }
        public bool CanMoveBackwards { get; private set; }
        public float MaximumWheelExtension { get; private set; }
        public float MaximumWheelCompression { get; private set; }
        public int FrontWheelTurnAngle { get; private set; }

        public int SlideIntoPlaceTime { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float RudderCorrectionDegree { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float RudderCorrectionRate { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float ElevatorCorrectionDegree { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float ElevatorCorrectionRate { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int TurnTime { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int TurnTimeDamaged { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float SlowTurnRadius { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float FastTurnRadius { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool NonDirtyTransform { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int PreferredAttackHeight { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float AeleronCorrectionDegree { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float AeleronCorrectionRate { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float SwoopStandoffRadius { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float SwoopStandoffHeight { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float SwoopTerminalVelocity { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float SwoopAccelerationRate { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float SwoopSpeedTuningFactor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public FormationPriority FormationPriority { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float BackingUpSpeed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ChargeAvailable { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float ChargeSpeed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float TurnThreshold { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int TurnThresholdHS { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool EnableHighSpeedTurnModelconditions { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool UseTerrainSmoothing { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool WaitForFormation { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MaxTurnWithoutReform { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float AccDecTrigger { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int WalkDistance { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MaxOverlappedHeight { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool CrewPowered { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool BackingUpStopWhenTurning { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int BackingUpDistanceMin { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int BackingUpDistanceMax { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float BackingUpAngle { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float LookAheadMult { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ScalesWalls { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ChargeIgnoresCondition { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int BurningDeathRadius { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool BurningDeathIsCavalry { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool TurnWhileMoving { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float RiverModifier { get; private set; }
    }

    public enum LocomotorAppearance
    {
        [IniEnum("TWO_LEGS")]
        TwoLegs,

        [IniEnum("CLIMBER")]
        Climber,

        [IniEnum("THRUST")]
        Thrust,

        [IniEnum("FOUR_WHEELS")]
        FourWheels,

        [IniEnum("HOVER")]
        Hover,

        [IniEnum("WINGS")]
        Wings,

        [IniEnum("TREADS")]
        Treads,

        [IniEnum("MOTORCYCLE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Motorcycle,

        [IniEnum("GIANT_BIRD"), AddedIn(SageGame.Bfme2)]
        GiantBird,

        [IniEnum("HUGE_TWO_LEGS"), AddedIn(SageGame.Bfme)]
        HugeTwoLegs,

        [IniEnum("HORDE"), AddedIn(SageGame.Bfme)]
        Horde,

        [IniEnum("FOUR_LEGS_HUGE"), AddedIn(SageGame.Bfme)]
        FourLegsHuge,

        [IniEnum("SHIP"), AddedIn(SageGame.Bfme2)]
        Ship,
    }

    public enum Surface
    {
        [IniEnum("GROUND")]
        Ground,

        [IniEnum("RUBBLE")]
        Rubble,

        [IniEnum("CLIFF")]
        Cliff,

        [IniEnum("AIR")]
        Air,

        [IniEnum("WATER")]
        Water,

        [IniEnum("IMPASSABLE"), AddedIn(SageGame.Bfme)]
        Impassable,

        [IniEnum("OBSTACLE"), AddedIn(SageGame.Bfme)]
        Obstacle,

        [IniEnum("DEEP_WATER"), AddedIn(SageGame.Bfme2)]
        DeepWater,
    }

    public enum GroupMovementPriority
    {
        [IniEnum("MOVES_FRONT")]
        MovesFront,

        [IniEnum("MOVES_MIDDLE")]
        MovesMiddle,

        [IniEnum("MOVES_BACK")]
        MovesBack
    }

    public enum LocomotorZAxisBehavior
    {
        [IniEnum("NO_Z_MOTIVE_FORCE")]
        NoZMotiveForce,

        [IniEnum("SURFACE_RELATIVE_HEIGHT")]
        SurfaceRelativeHeight,

        [IniEnum("RELATIVE_TO_HIGHEST_LAYER")]
        RelativeToHighestLayer,

        [IniEnum("ABSOLUTE_HEIGHT")]
        AbsoluteHeight,

        [IniEnum("FIXED_ABSOLUTE_HEIGHT"), AddedIn(SageGame.Bfme2)]
        FixedAbsoluteHeight,

        [IniEnum("FLOATING_Z"), AddedIn(SageGame.Bfme)]
        FloatingZ,

        [IniEnum("SEA_LEVEL"), AddedIn(SageGame.Bfme)]
        SeaLevel,

        [IniEnum("SCALING_WALLS"), AddedIn(SageGame.Bfme2)]
        ScalingWalls,
    }

    [AddedIn(SageGame.Bfme)]
    public enum FormationPriority
    {
        [IniEnum("NO_FORMATION")]
        NoFormation,

        [IniEnum("MELEE1")]
        Melee1,

        [IniEnum("MELEE2")]
        Melee2,
    }
}
