using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SlavedUpdate : ObjectBehavior
    {
        internal static SlavedUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SlavedUpdate> FieldParseTable = new IniParseTable<SlavedUpdate>
        {
            //{ "ToppleFX", (parser, x) => x.ToppleFX = parser.ParseAssetReference() }
        };
    }
}
