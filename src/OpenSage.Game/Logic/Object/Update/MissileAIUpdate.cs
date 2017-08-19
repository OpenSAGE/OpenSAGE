using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class MissileAIUpdate : ObjectBehavior
    {
        internal static MissileAIUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<MissileAIUpdate> FieldParseTable = new IniParseTable<MissileAIUpdate>
        {
            { "TryToFollowTarget", (parser, x) => x.TryToFollowTarget = parser.ParseBoolean() },
            { "FuelLifetime", (parser, x) => x.FuelLifetime = parser.ParseInteger() },
            { "InitialVelocity", (parser, x) => x.InitialVelocity = parser.ParseInteger() },
            { "IgnitionDelay", (parser, x) => x.IgnitionDelay = parser.ParseInteger() },
            { "DistanceToTravelBeforeTurning", (parser, x) => x.DistanceToTravelBeforeTurning = parser.ParseInteger() },
            { "DistanceToTargetBeforeDiving", (parser, x) => x.DistanceToTargetBeforeDiving = parser.ParseInteger() },
            { "DistanceToTargetForLock", (parser, x) => x.DistanceToTargetForLock = parser.ParseInteger() },
            { "IgnitionFX", (parser, x) => x.IgnitionFX = parser.ParseAssetReference() }
        };

        public bool TryToFollowTarget { get; private set; }
        public int FuelLifetime { get; private set; }
        public int InitialVelocity { get; private set; }
        public int IgnitionDelay { get; private set; }
        public int DistanceToTravelBeforeTurning { get; private set; }
        public int DistanceToTargetBeforeDiving { get; private set; }
        public int DistanceToTargetForLock { get; private set; }
        public string IgnitionFX { get; private set; }
    }
}
