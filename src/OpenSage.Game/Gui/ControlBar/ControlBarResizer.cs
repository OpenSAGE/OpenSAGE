using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Gui.ControlBar
{
    public sealed class ControlBarResizer : BaseAsset
    {
        internal static ControlBarResizer Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                 (x, name) => x.SetNameAndInstanceId("ControlBarResizer", name),
                 FieldParseTable);
        }

        private static readonly IniParseTable<ControlBarResizer> FieldParseTable = new IniParseTable<ControlBarResizer>
        {
            { "AltPosition", (parser, x) => x.AltPosition = parser.ParsePoint() },
            { "AltSize", (parser, x) => x.AltSize = parser.ParsePoint() }
        };

        public Point2D AltPosition { get; private set; }
        public Point2D AltSize { get; private set; }
    }
}
