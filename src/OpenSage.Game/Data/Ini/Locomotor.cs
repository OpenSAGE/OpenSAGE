using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class Locomotor
    {
        internal static Locomotor Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                 (x, name) => x.Name = name,
                 FieldParseTable);
        }

        private static readonly IniParseTable<Locomotor> FieldParseTable = new IniParseTable<Locomotor>
        {
            { "Surfaces", (parser, x) => x.Surfaces = parser.ParseEnumBitArray<Surface>() },
            { "Speed", (parser, x) => x.Speed = parser.ParseInteger() },
            { "SpeedDamaged", (parser, x) => x.SpeedDamaged = parser.ParseInteger() },
            { "MinSpeed", (parser, x) => x.MinSpeed = parser.ParseInteger() },
            { "TurnRate", (parser, x) => x.TurnRate = parser.ParseInteger() },
            { "TurnRateDamaged", (parser, x) => x.TurnRateDamaged = parser.ParseInteger() },
            { "Acceleration", (parser, x) => x.Acceleration = parser.ParseInteger() },
            { "AccelerationDamaged", (parser, x) => x.AccelerationDamaged = parser.ParseInteger() },
            { "Lift", (parser, x) => x.Lift = parser.ParseInteger() },
            { "LiftDamaged", (parser, x) => x.LiftDamaged = parser.ParseInteger() },
            { "Braking", (parser, x) => x.Braking = parser.ParseInteger() },
            { "MinTurnSpeed", (parser, x) => x.MinTurnSpeed = parser.ParseInteger() },
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
        };

        public string Name { get; private set; }

        public BitArray<Surface> Surfaces { get; private set; }
        public int Speed { get; private set; }
        public int SpeedDamaged { get; private set; }
        public int MinSpeed { get; private set; }
        public int TurnRate { get; private set; }
        public int TurnRateDamaged { get; private set; }
        public int Acceleration { get; private set; }
        public int AccelerationDamaged { get; private set; }
        public int Lift { get; private set; }
        public int LiftDamaged { get; private set; }
        public int Braking { get; private set; }
        public int MinTurnSpeed { get; private set; }
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
        Treads
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
        Water
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
        AbsoluteHeight
    }
}
