using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class GrantScienceUpgrade : ObjectBehavior
    {
        internal static GrantScienceUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GrantScienceUpgrade> FieldParseTable = new IniParseTable<GrantScienceUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "GrantScience", (parser, x) => x.GrantScience = parser.ParseAssetReference() },
        };

        public string TriggeredBy { get; private set; }
        public string GrantScience { get; private set; }
    }
}
