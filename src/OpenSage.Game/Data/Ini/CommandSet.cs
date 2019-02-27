using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class CommandSet
    {
        internal static void Parse(IniParser parser, IniDataContext context) => parser.ParseBlockContent(
            (x, name) => x.Name = name,
            context.CommandSets,
            FieldParseTable,
            new IniArbitraryFieldParserProvider<CommandSet>(
                    (x, name) => x.Buttons[int.Parse(name)] = parser.ParseAssetReference()));

        private static readonly IniParseTable<CommandSet> FieldParseTable = new IniParseTable<CommandSet>
        {
            { "InitialVisible", (parser, x) => x.InitialVisible = parser.ParseInteger() },
        };

        public string Name { get; private set; }

        // These are laid out like this:
        // -------------------------------
        // | 01 | 03 | 05 | 07 | 09 | 11 |
        // -------------------------------
        // | 02 | 04 | 06 | 08 | 10 | 12 |
        // -------------------------------
        public Dictionary<int, string> Buttons { get; } = new Dictionary<int, string>();

        [AddedIn(SageGame.Bfme2)]
        public int InitialVisible { get; private set; }
    }
}
