using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
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
                { "AttackLocomotorType", (parser, x) => x.AttackLocomotorType = parser.ParseEnum<LocomotorSetCondition>() },
                { "AttackLocomotorPersistTime", (parser, x) => x.AttackLocomotorPersistTime = parser.ParseInteger() },
                { "AttackersMissPersistTime", (parser, x) => x.AttackersMissPersistTime = parser.ParseInteger() },
                { "ReturnForAmmoLocomotorType", (parser, x) => x.ReturnForAmmoLocomotorType = parser.ParseEnum<LocomotorSetCondition>() },
                { "ParkingOffset", (parser, x) => x.ParkingOffset = parser.ParseInteger() },
                { "ReturnToBaseIdleTime", (parser, x) => x.ReturnToBaseIdleTime = parser.ParseInteger() },
            });

        /// <summary>
        /// Amount of damage, as a percentage of max health, to take per second when out of ammo.
        /// </summary>
        public Percentage OutOfAmmoDamagePerSecond { get; private set; }

        public Percentage TakeoffSpeedForMaxLift { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public Percentage TakeoffDistForMaxLift { get; private set; }

        public int TakeoffPause { get; private set; }
        public int MinHeight { get; private set; }
        public bool NeedsRunway { get; private set; }
        public bool KeepsParkingSpaceWhenAirborne { get; private set; }
        public float SneakyOffsetWhenAttacking { get; private set; }
        public LocomotorSetCondition AttackLocomotorType { get; private set; }
        public int AttackLocomotorPersistTime { get; private set; }
        public int AttackersMissPersistTime { get; private set; }
        public LocomotorSetCondition ReturnForAmmoLocomotorType { get; private set; }
        public int ParkingOffset { get; private set; }
        public int ReturnToBaseIdleTime { get; private set; }
    }
}
