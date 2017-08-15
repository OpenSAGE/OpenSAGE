using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class MultiplayerColor
    {
        internal static MultiplayerColor Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<MultiplayerColor> FieldParseTable = new IniParseTable<MultiplayerColor>
        {
            { "RGBColor", (parser, x) => x.RgbColor = IniColorRgb.Parse(parser) },
            { "RGBNightColor", (parser, x) => x.RgbNightColor = IniColorRgb.Parse(parser) },
            { "TooltipName", (parser, x) => x.TooltipName = parser.ParseLocalizedStringKey() }
        };

        public string Name { get; private set; }

        public IniColorRgb RgbColor { get; private set; }
        public IniColorRgb RgbNightColor { get; private set; }
        public string TooltipName { get; private set; }
    }
}
