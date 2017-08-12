using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class InGameUI
    {
        internal static InGameUI Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<InGameUI> FieldParseTable = new IniParseTable<InGameUI>
        {
            //{ "ShellMapName", (parser, x) => x.ShellMapName = parser.ParseAsciiString() },
        };
    }
}
