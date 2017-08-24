using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class CommandSet
    {
        internal static CommandSet Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                new IniArbitraryFieldParserProvider<CommandSet>(
                    (x, name) => x.Buttons[int.Parse(name)] = parser.ParseAssetReference()));
        }

        public string Name { get; private set; }

        // These are laid out like this:
        // -------------------------------
        // | 01 | 03 | 05 | 07 | 09 | 11 |
        // -------------------------------
        // | 02 | 04 | 06 | 08 | 10 | 12 |
        // -------------------------------
        public Dictionary<int, string> Buttons { get; } = new Dictionary<int, string>();
    }
}
