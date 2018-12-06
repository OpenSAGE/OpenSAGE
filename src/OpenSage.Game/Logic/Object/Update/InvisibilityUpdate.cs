using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class InvisibilityUpdateModuleData : UpdateModuleData
    {
        internal static InvisibilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InvisibilityUpdateModuleData> FieldParseTable = new IniParseTable<InvisibilityUpdateModuleData>
        {
            { "UpdatePeriod", (parser, x) => x.UpdatePeriod = parser.ParseInteger() },
            { "StartsActive", (parser, x) => x.StartsActive  = parser.ParseBoolean() },
            { "InvisibilityNugget", (parser, x) => x.InvisibilityNugget = InvisibilityNugget.Parse(parser) }
        };

        public int UpdatePeriod { get; private set; }
        public bool StartsActive { get; private set; }
        public InvisibilityNugget InvisibilityNugget { get; private set; }
    }

    public sealed class InvisibilityNugget
    {
        internal static InvisibilityNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InvisibilityNugget> FieldParseTable = new IniParseTable<InvisibilityNugget>
        {
            { "InvisibilityType", (parser, x) => x.Type = parser.ParseEnum<InvisibilityType>() },
            { "DetectionRange", (parser, x) => x.DetectionRange  = parser.ParseInteger() },
            { "Options", (parser, x) => x.Options = parser.ParseEnum<InvisibilityOptions>() },
        };

        public InvisibilityType Type { get; private set; }
        public int DetectionRange { get; private set; }
        public InvisibilityOptions Options { get; private set; }
    }

    public enum InvisibilityType
    {
        [IniEnum("CAMOUFLAGE")]
        Camouflage,
    }

    public enum InvisibilityOptions
    {
        [IniEnum("DETECTED_BY_FRIENDLIES")]
        DetectedByFriendlies,
    }
}
