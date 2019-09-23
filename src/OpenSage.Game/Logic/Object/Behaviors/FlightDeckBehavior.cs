using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires <see cref="ObjectKinds.AircraftCarrier"/> and <see cref="ObjectKinds.FSAirfield"/> 
    /// kinds.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class FlightDeckBehaviorModuleData : BehaviorModuleData
    {
        internal static FlightDeckBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FlightDeckBehaviorModuleData> FieldParseTable = new IniParseTable<FlightDeckBehaviorModuleData>
        {
            { "NumRunways", (parser, x) => x.NumRunways = parser.ParseInteger() },
            { "NumSpacesPerRunway", (parser, x) => x.NumSpacesPerRunway = parser.ParseInteger() },

            { "Runway1Spaces", (parser, x) => x.Runway1Spaces = parser.ParseBoneNameArray() },
            { "Runway1Takeoff", (parser, x) => x.Runway1Takeoff = parser.ParseBoneNameArray() },
            { "Runway1Landing", (parser, x) => x.Runway1Landing = parser.ParseBoneNameArray() },
            { "Runway1Taxi", (parser, x) => x.Runway1Taxi = parser.ParseBoneNameArray() },
            { "Runway1Creation", (parser, x) => x.Runway1Creation = parser.ParseBoneNameArray() },
            { "Runway1CatapultSystem", (parser, x) => x.Runway1CatapultSystem = parser.ParseAssetReference() },

            { "Runway2Spaces", (parser, x) => x.Runway2Spaces = parser.ParseBoneNameArray() },
            { "Runway2Takeoff", (parser, x) => x.Runway2Takeoff = parser.ParseBoneNameArray() },
            { "Runway2Landing", (parser, x) => x.Runway2Landing = parser.ParseBoneNameArray() },
            { "Runway2Taxi", (parser, x) => x.Runway2Taxi = parser.ParseBoneNameArray() },
            { "Runway2Creation", (parser, x) => x.Runway2Creation = parser.ParseBoneNameArray() },
            { "Runway2CatapultSystem", (parser, x) => x.Runway2CatapultSystem = parser.ParseAssetReference() },

            { "HealAmountPerSecond", (parser, x) => x.HealAmountPerSecond = parser.ParseInteger() },

            { "ApproachHeight", (parser, x) => x.ApproachHeight = parser.ParseInteger() },
            { "LandingDeckHeightOffset", (parser, x) => x.LandingDeckHeightOffset = parser.ParseFloat() },
            { "ParkingCleanupPeriod", (parser, x) => x.ParkingCleanupPeriod = parser.ParseInteger() },
            { "HumanFollowPeriod", (parser, x) => x.HumanFollowPeriod = parser.ParseInteger() },

            { "PayloadTemplate", (parser, x) => x.PayloadTemplate = parser.ParseAssetReference() },
            { "ReplacementDelay", (parser, x) => x.ReplacementDelay = parser.ParseInteger() },
            { "DockAnimationDelay", (parser, x) => x.DockAnimationDelay = parser.ParseInteger() },

            { "LaunchWaveDelay", (parser, x) => x.LaunchWaveDelay = parser.ParseInteger() },
            { "LaunchRampDelay", (parser, x) => x.LaunchRampDelay = parser.ParseInteger() },
            { "LowerRampDelay", (parser, x) => x.LowerRampDelay = parser.ParseInteger() },
            { "CatapultFireDelay", (parser, x) => x.CatapultFireDelay = parser.ParseInteger() },
        };

        public int NumRunways { get; private set; }
        public int NumSpacesPerRunway { get; private set; }

        public string[] Runway1Spaces { get; private set; }
        public string[] Runway1Takeoff { get; private set; }
        public string[] Runway1Landing { get; private set; }
        public string[] Runway1Taxi { get; private set; }
        public string[] Runway1Creation { get; private set; }
        public string Runway1CatapultSystem { get; private set; }

        public string[] Runway2Spaces { get; private set; }
        public string[] Runway2Takeoff { get; private set; }
        public string[] Runway2Landing { get; private set; }
        public string[] Runway2Taxi { get; private set; }
        public string[] Runway2Creation { get; private set; }
        public string Runway2CatapultSystem { get; private set; }

        /// <summary>
        /// Amount of health to give non-airborne aircraft on the deck.
        /// </summary>
        public int HealAmountPerSecond { get; private set; }

        public int ApproachHeight { get; private set; }
        public float LandingDeckHeightOffset { get; private set; }
        public int ParkingCleanupPeriod { get; private set; }
        public int HumanFollowPeriod { get; private set; }

        public string PayloadTemplate { get; private set; }
        public int ReplacementDelay { get; private set; }
        public int DockAnimationDelay { get; private set; }

        public int LaunchWaveDelay { get; private set; }
        public int LaunchRampDelay { get; private set; }
        public int LowerRampDelay { get; private set; }
        public int CatapultFireDelay { get; private set; }
    }
}
