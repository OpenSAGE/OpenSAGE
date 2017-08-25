using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class CountermeasuresBehavior : ObjectBehavior
    {
        internal static CountermeasuresBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CountermeasuresBehavior> FieldParseTable = new IniParseTable<CountermeasuresBehavior>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() },
            { "FlareTemplateName", (parser, x) => x.FlareTemplateName = parser.ParseAssetReference() },
            { "FlareBoneBaseName", (parser, x) => x.FlareBoneBaseName = parser.ParseBoneName() },
            { "VolleySize", (parser, x) => x.VolleySize = parser.ParseInteger() },
            { "VolleyArcAngle", (parser, x) => x.VolleyArcAngle = parser.ParseFloat() },
            { "VolleyVelocityFactor", (parser, x) => x.VolleyVelocityFactor = parser.ParseFloat() },
            { "DelayBetweenVolleys", (parser, x) => x.DelayBetweenVolleys = parser.ParseInteger() },
            { "NumberOfVolleys", (parser, x) => x.NumberOfVolleys = parser.ParseInteger() },
            { "ReloadTime", (parser, x) => x.ReloadTime = parser.ParseInteger() },
            { "EvasionRate", (parser, x) => x.EvasionRate = parser.ParsePercentage() },
            { "ReactionLaunchLatency", (parser, x) => x.ReactionLaunchLatency = parser.ParseInteger() },
            { "MissileDecoyDelay", (parser, x) => x.MissileDecoyDelay = parser.ParseInteger() },
        };

        public string[] TriggeredBy { get; private set; }
        public string FlareTemplateName { get; private set; }
        public string FlareBoneBaseName { get; private set; }
        public int VolleySize { get; private set; }
        public float VolleyArcAngle { get; private set; }
        public float VolleyVelocityFactor { get; private set; }
        public int DelayBetweenVolleys { get; private set; }
        public int NumberOfVolleys { get; private set; }
        public int ReloadTime { get; private set; }
        public float EvasionRate { get; private set; }
        public int ReactionLaunchLatency { get; private set; }
        public int MissileDecoyDelay { get; private set; }
    }
}
