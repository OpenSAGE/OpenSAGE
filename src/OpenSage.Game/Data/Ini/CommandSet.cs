using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
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
            { "1", (parser, x) => x.Buttons[0] = parser.ParseAssetReference() },
            { "2", (parser, x) => x.Buttons[1] = parser.ParseAssetReference() },
            { "3", (parser, x) => x.Buttons[2] = parser.ParseAssetReference() },
            { "4", (parser, x) => x.Buttons[3] = parser.ParseAssetReference() },
            { "5", (parser, x) => x.Buttons[4] = parser.ParseAssetReference() },
            { "6", (parser, x) => x.Buttons[5] = parser.ParseAssetReference() },
            { "7", (parser, x) => x.Buttons[6] = parser.ParseAssetReference() },
            { "8", (parser, x) => x.Buttons[7] = parser.ParseAssetReference() },
            { "9", (parser, x) => x.Buttons[8] = parser.ParseAssetReference() },
            { "10", (parser, x) => x.Buttons[9] = parser.ParseAssetReference() },
            { "11", (parser, x) => x.Buttons[10] = parser.ParseAssetReference() },
            { "12", (parser, x) => x.Buttons[11] = parser.ParseAssetReference() }
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
