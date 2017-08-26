using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class ReplaceObjectUpgrade : ObjectBehavior
    {
        internal static ReplaceObjectUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ReplaceObjectUpgrade> FieldParseTable = new IniParseTable<ReplaceObjectUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "ReplaceObject", (parser, x) => x.ReplaceObject = parser.ParseAssetReference() },
        };

        public string TriggeredBy { get; private set; }
        public string ReplaceObject { get; private set; }
    }
}
