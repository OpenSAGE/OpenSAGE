using OpenZH.Data.Ini.Parser;
using OpenZH.Data.Wnd;

namespace OpenZH.Data.Ini
{
    public sealed class Language
    {
        internal static Language Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<Language> FieldParseTable = new IniParseTable<Language>
        {
            { "UnicodeFontName", (parser, x) => x.UnicodeFontName = parser.ParseAsciiString() },
            { "MilitaryCaptionSpeed", (parser, x) => x.MilitaryCaptionSpeed = parser.ParseInteger() },
            { "CopyrightFont", (parser, x) => x.CopyrightFont = parser.ParseFont() },
            { "MessageFont", (parser, x) => x.MessageFont = parser.ParseFont() },
            { "MilitaryCaptionTitleFont", (parser, x) => x.MilitaryCaptionTitleFont = parser.ParseFont() },
            { "MilitaryCaptionFont", (parser, x) => x.MilitaryCaptionFont = parser.ParseFont() },
            { "SuperweaponCountdownNormalFont", (parser, x) => x.SuperweaponCountdownNormalFont = parser.ParseFont() },
            { "SuperweaponCountdownReadyFont", (parser, x) => x.SuperweaponCountdownReadyFont = parser.ParseFont() },
            { "NamedTimerCountdownNormalFont", (parser, x) => x.NamedTimerCountdownNormalFont = parser.ParseFont() },
            { "NamedTimerCountdownReadyFont", (parser, x) => x.NamedTimerCountdownReadyFont = parser.ParseFont() },
            { "DrawableCaptionFont", (parser, x) => x.DrawableCaptionFont = parser.ParseFont() },
            { "DefaultWindowFont", (parser, x) => x.DefaultWindowFont = parser.ParseFont() },
            { "DefaultDisplayStringFont", (parser, x) => x.DefaultDisplayStringFont = parser.ParseFont() },
            { "TooltipFontName", (parser, x) => x.TooltipFontName = parser.ParseFont() },
            { "NativeDebugDisplay", (parser, x) => x.NativeDebugDisplay = parser.ParseFont() },
            { "DrawGroupInfoFont", (parser, x) => x.DrawGroupInfoFont = parser.ParseFont() },
            { "CreditsTitleFont", (parser, x) => x.CreditsTitleFont = parser.ParseFont() },
            { "CreditsMinorTitleFont", (parser, x) => x.CreditsMinorTitleFont = parser.ParseFont() },
            { "CreditsNormalFont", (parser, x) => x.CreditsNormalFont = parser.ParseFont() },
            { "ResolutionFontAdjustment", (parser, x) => x.ResolutionFontAdjustment = parser.ParseFloat() },
        };

        public string UnicodeFontName { get; private set; }
        public int MilitaryCaptionSpeed { get; private set; }
        public WndFont CopyrightFont { get; private set; }
        public WndFont MessageFont { get; private set; }
        public WndFont MilitaryCaptionTitleFont { get; private set; }
        public WndFont MilitaryCaptionFont { get; private set; }
        public WndFont SuperweaponCountdownNormalFont { get; private set; }
        public WndFont SuperweaponCountdownReadyFont { get; private set; }
        public WndFont NamedTimerCountdownNormalFont { get; private set; }
        public WndFont NamedTimerCountdownReadyFont { get; private set; }
        public WndFont DrawableCaptionFont { get; private set; }
        public WndFont DefaultWindowFont { get; private set; }
        public WndFont DefaultDisplayStringFont { get; private set; }
        public WndFont TooltipFontName { get; private set; }
        public WndFont NativeDebugDisplay { get; private set; }
        public WndFont DrawGroupInfoFont { get; private set; }
        public WndFont CreditsTitleFont { get; private set; }
        public WndFont CreditsMinorTitleFont { get; private set; }
        public WndFont CreditsNormalFont { get; private set; }
        public float ResolutionFontAdjustment { get; private set; }
    }
}
