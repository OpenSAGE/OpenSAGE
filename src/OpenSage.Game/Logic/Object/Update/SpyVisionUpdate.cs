using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SpyVisionUpdate : ObjectBehavior
    {
        internal static SpyVisionUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpyVisionUpdate> FieldParseTable = new IniParseTable<SpyVisionUpdate>();
    }
}
