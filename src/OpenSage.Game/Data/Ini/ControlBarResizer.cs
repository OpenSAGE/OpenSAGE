using OpenSage.Data.Ini.Parser;
using OpenSage.Data.Wnd;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    public sealed class ControlBarResizer
    {
        internal static ControlBarResizer Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                 (x, name) => x.Name = name,
                 FieldParseTable);
        }

        private static readonly IniParseTable<ControlBarResizer> FieldParseTable = new IniParseTable<ControlBarResizer>
        {
            { "AltPosition", (parser, x) => x.AltPosition = parser.ParsePoint() },
            { "AltSize", (parser, x) => x.AltSize = parser.ParsePoint() }
        };

        public string Name { get; private set; }

        public Point2D AltPosition { get; private set; }
        public Point2D AltSize { get; private set; }
    }
}
