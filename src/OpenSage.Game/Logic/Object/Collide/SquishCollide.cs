using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SquishCollideModuleData : CollideModuleData
    {
        internal static SquishCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SquishCollideModuleData> FieldParseTable = new IniParseTable<SquishCollideModuleData>();
    }
}
