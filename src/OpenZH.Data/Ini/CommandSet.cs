using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class CommandSet
    {
        internal static CommandSet Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                 (x, name) => x.Name = name,
                 FieldParseTable);
        }

        private static readonly IniParseTable<CommandSet> FieldParseTable = new IniParseTable<CommandSet>
        {
            { "1", (parser, x) => x.Buttons[0] = parser.ParseAsciiString() },
            { "2", (parser, x) => x.Buttons[1] = parser.ParseAsciiString() },
            { "3", (parser, x) => x.Buttons[2] = parser.ParseAsciiString() },
            { "4", (parser, x) => x.Buttons[3] = parser.ParseAsciiString() },
            { "5", (parser, x) => x.Buttons[4] = parser.ParseAsciiString() },
            { "6", (parser, x) => x.Buttons[5] = parser.ParseAsciiString() },
            { "7", (parser, x) => x.Buttons[6] = parser.ParseAsciiString() },
            { "8", (parser, x) => x.Buttons[7] = parser.ParseAsciiString() },
            { "9", (parser, x) => x.Buttons[8] = parser.ParseAsciiString() },
            { "10", (parser, x) => x.Buttons[9] = parser.ParseAsciiString() },
            { "11", (parser, x) => x.Buttons[10] = parser.ParseAsciiString() },
            { "12", (parser, x) => x.Buttons[11] = parser.ParseAsciiString() }
        };

        public string Name { get; private set; }

        // These are laid out like this:
        // -------------------------------
        // | 01 | 03 | 05 | 07 | 09 | 11 |
        // -------------------------------
        // | 02 | 04 | 06 | 08 | 10 | 12 |
        // -------------------------------
        public string[] Buttons { get; } = new string[12];
    }
}
