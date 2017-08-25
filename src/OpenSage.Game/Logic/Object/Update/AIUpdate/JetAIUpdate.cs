using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of VoiceLowFuel and Afterburner within UnitSpecificSounds section of the object.
    /// Requires Kindof = AIRCRAFT.
    /// Allows the use of JETEXHAUST JETAFTERBURNER model condition states; this is triggered when
    /// it's taking off from the runway.
    /// </summary>
    public sealed class JetAIUpdate : ObjectBehavior
    {
        internal static JetAIUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<JetAIUpdate> FieldParseTable = new IniParseTable<JetAIUpdate>
        {
            { "OutOfAmmoDamagePerSecond", (parser, x) => x.OutOfAmmoDamagePerSecond = parser.ParsePercentage() },
            { "TakeoffSpeedForMaxLift", (parser, x) => x.TakeoffSpeedForMaxLift = parser.ParsePercentage() },
            { "TakeoffDistForMaxLift", (parser, x) => x.TakeoffDistForMaxLift = parser.ParsePercentage() },
            { "TakeoffPause", (parser, x) => x.TakeoffPause = parser.ParseInteger() },
            { "MinHeight", (parser, x) => x.MinHeight = parser.ParseInteger() },
            { "NeedsRunway", (parser, x) => x.NeedsRunway = parser.ParseBoolean() },
            { "KeepsParkingSpaceWhenAirborne", (parser, x) => x.KeepsParkingSpaceWhenAirborne = parser.ParseBoolean() },
            { "AutoAcquireEnemiesWhenIdle", (parser, x) => x.AutoAcquireEnemiesWhenIdle = parser.ParseBoolean() },
            { "SneakyOffsetWhenAttacking", (parser, x) => x.SneakyOffsetWhenAttacking = parser.ParseFloat() },
            { "AttackLocomotorType", (parser, x) => x.AttackLocomotorType = parser.ParseEnum<LocomotorSet>() },
            { "AttackLocomotorPersistTime", (parser, x) => x.AttackLocomotorPersistTime = parser.ParseInteger() },
            { "AttackersMissPersistTime", (parser, x) => x.AttackersMissPersistTime = parser.ParseInteger() },
            { "ReturnForAmmoLocomotorType", (parser, x) => x.ReturnForAmmoLocomotorType = parser.ParseEnum<LocomotorSet>() },
            { "ParkingOffset", (parser, x) => x.ParkingOffset = parser.ParseInteger() },
            { "ReturnToBaseIdleTime", (parser, x) => x.ReturnToBaseIdleTime = parser.ParseInteger() },
            { "Turret", (parser, x) => x.Turret = TurretAIData.Parse(parser) },
        };

        /// <summary>
        /// Amount of damage, as a percentage of max health, to take per second when out of ammo.
        /// </summary>
        public float OutOfAmmoDamagePerSecond { get; private set; }

        public float TakeoffSpeedForMaxLift { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float TakeoffDistForMaxLift { get; private set; }

        public int TakeoffPause { get; private set; }
        public int MinHeight { get; private set; }
        public bool NeedsRunway { get; private set; }
        public bool KeepsParkingSpaceWhenAirborne { get; private set; }
        public bool AutoAcquireEnemiesWhenIdle { get; private set; }
        public float SneakyOffsetWhenAttacking { get; private set; }
        public LocomotorSet AttackLocomotorType { get; private set; }
        public int AttackLocomotorPersistTime { get; private set; }
        public int AttackersMissPersistTime { get; private set; }
        public LocomotorSet ReturnForAmmoLocomotorType { get; private set; }
        public int ParkingOffset { get; private set; }
        public int ReturnToBaseIdleTime { get; private set; }

        public TurretAIData Turret { get; private set; }
    }
}
