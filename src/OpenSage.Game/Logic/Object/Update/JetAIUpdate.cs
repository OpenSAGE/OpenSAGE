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
            { "TakeoffPause", (parser, x) => x.TakeoffPause = parser.ParseInteger() },
            { "MinHeight", (parser, x) => x.MinHeight = parser.ParseInteger() },
            { "ParkingOffset", (parser, x) => x.ParkingOffset = parser.ParseInteger() },
            { "ReturnToBaseIdleTime", (parser, x) => x.ReturnToBaseIdleTime = parser.ParseInteger() }
        };

        /// <summary>
        /// Amount of damage, as a percentage of max health, to take per second when out of ammo.
        /// </summary>
        public float OutOfAmmoDamagePerSecond { get; private set; }

        public float TakeoffSpeedForMaxLift { get; private set; }
        public int TakeoffPause { get; private set; }
        public int MinHeight { get; private set; }
        public int ParkingOffset { get; private set; }
        public int ReturnToBaseIdleTime { get; private set; }
    }
}
