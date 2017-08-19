using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class LaserUpdate : ClientUpdate
    {
        internal static LaserUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LaserUpdate> FieldParseTable = new IniParseTable<LaserUpdate>();
    }
}
