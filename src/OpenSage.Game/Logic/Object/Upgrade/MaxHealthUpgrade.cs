using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class MaxHealthUpgrade : ObjectBehavior
    {
        internal static MaxHealthUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<MaxHealthUpgrade> FieldParseTable = new IniParseTable<MaxHealthUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "AddMaxHealth", (parser, x) => x.AddMaxHealth = parser.ParseFloat() },
            { "ChangeType", (parser, x) => x.ChangeType = parser.ParseEnum<MaxHealthChangeType>() },
        };

        public string TriggeredBy { get; private set; }
        public float AddMaxHealth { get; private set; }
        public MaxHealthChangeType ChangeType { get; private set; }
    }

    public enum MaxHealthChangeType
    {
        [IniEnum("PRESERVE_RATIO")]
        PreserveRatio,

        [IniEnum("ADD_CURRENT_HEALTH_TOO")]
        AddCurrentHealthToo
    }
}
