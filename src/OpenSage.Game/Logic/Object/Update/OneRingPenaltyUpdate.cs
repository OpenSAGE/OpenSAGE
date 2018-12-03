using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class OneRingPenaltyUpdateModuleData : UpdateModuleData
    {
        internal static OneRingPenaltyUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<OneRingPenaltyUpdateModuleData> FieldParseTable = new IniParseTable<OneRingPenaltyUpdateModuleData>
        {
            { "SpecialObjectName", (parser, x) => x.SpecialObjectName = parser.ParseAssetReference() },
            { "RingTimeBeforeSpawning", (parser, x) => x.RingTimeBeforeSpawning = parser.ParseInteger() },
            { "TimeSpentRoamingAround", (parser, x) => x.TimeSpentRoamingAround = parser.ParseInteger() },
            { "TimeRingPowerSuppressed", (parser, x) => x.TimeRingPowerSuppressed = parser.ParseInteger() },
            { "StartingDistanceFromMe", (parser, x) => x.StartingDistanceFromMe = parser.ParseInteger() },
            { "TimeFrozenFromPenalty", (parser, x) => x.TimeFrozenFromPenalty = parser.ParseInteger() },
            { "DiscoveredSound", (parser, x) => x.DiscoveredSound = parser.ParseAssetReference() },
        };

        public string SpecialObjectName { get; private set; }
        public int RingTimeBeforeSpawning { get; private set; }
        public int TimeSpentRoamingAround { get; private set; }
        public int TimeRingPowerSuppressed { get; private set; }
        public int StartingDistanceFromMe { get; private set; }
        public int TimeFrozenFromPenalty { get; private set; }
        public string DiscoveredSound { get; private set; }
    }
}
