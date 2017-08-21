using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class HijackerUpdate : ObjectBehavior
    {
        internal static HijackerUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<HijackerUpdate> FieldParseTable = new IniParseTable<HijackerUpdate>
        {
            { "ParachuteName", (parser, x) => x.ParachuteName = parser.ParseAssetReference() }
        };

        public string ParachuteName { get; private set; }
    }
}
