using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Gui.ControlBar
{
    public sealed class CommandSet : BaseAsset
    {
        internal static CommandSet Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("CommandSet", name),
                FieldParseTable,
                new IniArbitraryFieldParserProvider<CommandSet>(
                    (x, name) => x.Buttons[int.Parse(name)] = parser.ParseCommandButtonReference()));
        }

        private static readonly IniParseTable<CommandSet> FieldParseTable = new IniParseTable<CommandSet>
        {
            { "InitialVisible", (parser, x) => x.InitialVisible = parser.ParseInteger() },
        };

        // These are laid out like this:
        // -------------------------------
        // | 01 | 03 | 05 | 07 | 09 | 11 |
        // -------------------------------
        // | 02 | 04 | 06 | 08 | 10 | 12 |
        // -------------------------------
        public Dictionary<int, LazyAssetReference<CommandButton>> Buttons { get; } = new Dictionary<int, LazyAssetReference<CommandButton>>();

        [AddedIn(SageGame.Bfme2)]
        public int InitialVisible { get; private set; }
    }
}
