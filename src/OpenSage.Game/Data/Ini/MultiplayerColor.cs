using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
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
            { "TooltipName", (parser, x) => x.TooltipName = parser.ParseLocalizedStringKey() },
            { "LivingWorldColor", (parser, x) => x.LivingWorldColor = parser.ParseColorRgb() },
            { "LivingWorldBannerColor", (parser, x) => x.LivingWorldBannerColor = parser.ParseColorRgb() },
            { "AvailableInWotR", (parser, x) => x.AvailableInWotR = parser.ParseBoolean() }
        };

        public string Name { get; private set; }

        public IniColorRgb RgbColor { get; private set; }
        public IniColorRgb RgbNightColor { get; private set; }
        public string TooltipName { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ColorRgb LivingWorldColor { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ColorRgb LivingWorldBannerColor { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool AvailableInWotR { get; private set; }
    }
}
