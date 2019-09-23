using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Gui.InGame
{
    public sealed class DrawGroupInfo
    {
        internal static DrawGroupInfo Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<DrawGroupInfo> FieldParseTable = new IniParseTable<DrawGroupInfo>
        {
            { "UsePlayerColor", (parser, x) => x.UsePlayerColor = parser.ParseBoolean() },
            { "ColorForText", (parser, x) => x.ColorForText = parser.ParseColorRgba() },
            { "ColorForTextDropShadow", (parser, x) => x.ColorForTextDropShadow = parser.ParseColorRgba() },

            { "DropShadowOffsetX", (parser, x) => x.DropShadowOffsetX = parser.ParseInteger() },
            { "DropShadowOffsetY", (parser, x) => x.DropShadowOffsetY = parser.ParseInteger() },

            { "FontName", (parser, x) => x.FontName = parser.ParseString() },
            { "FontSize", (parser, x) => x.FontSize = parser.ParseInteger() },
            { "FontIsBold", (parser, x) => x.FontIsBold = parser.ParseBoolean() },

            { "DrawPositionXPercent", (parser, x) => x.DrawPositionXPercent = parser.ParsePercentage() },
            { "DrawPositionYPixel", (parser, x) => x.DrawPositionYPixel = parser.ParseInteger() },
        };

        public bool UsePlayerColor { get; private set; }
        public ColorRgba ColorForText { get; private set; }
        public ColorRgba ColorForTextDropShadow { get; private set; }

        public int DropShadowOffsetX { get; private set; }
        public int DropShadowOffsetY { get; private set; }

        public string FontName { get; private set; }
        public int FontSize { get; private set; }
        public bool FontIsBold { get; private set; }

        public Percentage DrawPositionXPercent { get; private set; }
        public float DrawPositionYPixel { get; private set; }
    }
}
