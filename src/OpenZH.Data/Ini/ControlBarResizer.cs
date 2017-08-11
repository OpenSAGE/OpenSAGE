using OpenZH.Data.Ini.Parser;
using OpenZH.Data.Wnd;

namespace OpenZH.Data.Ini
{
    public sealed class ControlBarResizer
    {
        internal static ControlBarResizer Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                 (x, name) => x.Name = name,
                 FieldParseTable);
        }

        private static readonly IniParseTable<ControlBarResizer> FieldParseTable = new IniParseTable<ControlBarResizer>
        {
            { "AltPosition", (parser, x) => x.AltPosition = WndPoint.Parse(parser) },
            { "AltSize", (parser, x) => x.AltSize = WndPoint.Parse(parser) }
        };

        public string Name { get; private set; }

        public WndPoint AltPosition { get; private set; }
        public WndPoint AltSize { get; private set; }
    }
}
