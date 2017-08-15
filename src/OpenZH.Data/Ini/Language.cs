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
            { "UnicodeFontName", (parser, x) => x.UnicodeFontName = parser.ParseString() },
            { "MilitaryCaptionSpeed", (parser, x) => x.MilitaryCaptionSpeed = parser.ParseInteger() },
            { "CopyrightFont", (parser, x) => x.CopyrightFont = WndFont.Parse(parser) },
            { "MessageFont", (parser, x) => x.MessageFont = WndFont.Parse(parser) },
            { "MilitaryCaptionTitleFont", (parser, x) => x.MilitaryCaptionTitleFont = WndFont.Parse(parser) },
            { "MilitaryCaptionFont", (parser, x) => x.MilitaryCaptionFont = WndFont.Parse(parser) },
            { "SuperweaponCountdownNormalFont", (parser, x) => x.SuperweaponCountdownNormalFont = WndFont.Parse(parser) },
            { "SuperweaponCountdownReadyFont", (parser, x) => x.SuperweaponCountdownReadyFont = WndFont.Parse(parser) },
            { "NamedTimerCountdownNormalFont", (parser, x) => x.NamedTimerCountdownNormalFont = WndFont.Parse(parser) },
            { "NamedTimerCountdownReadyFont", (parser, x) => x.NamedTimerCountdownReadyFont = WndFont.Parse(parser) },
            { "DrawableCaptionFont", (parser, x) => x.DrawableCaptionFont = WndFont.Parse(parser) },
            { "DefaultWindowFont", (parser, x) => x.DefaultWindowFont = WndFont.Parse(parser) },
            { "DefaultDisplayStringFont", (parser, x) => x.DefaultDisplayStringFont = WndFont.Parse(parser) },
            { "TooltipFontName", (parser, x) => x.TooltipFontName = WndFont.Parse(parser) },
            { "NativeDebugDisplay", (parser, x) => x.NativeDebugDisplay = WndFont.Parse(parser) },
            { "DrawGroupInfoFont", (parser, x) => x.DrawGroupInfoFont = WndFont.Parse(parser) },
            { "CreditsTitleFont", (parser, x) => x.CreditsTitleFont = WndFont.Parse(parser) },
            { "CreditsMinorTitleFont", (parser, x) => x.CreditsMinorTitleFont = WndFont.Parse(parser) },
            { "CreditsNormalFont", (parser, x) => x.CreditsNormalFont = WndFont.Parse(parser) },
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
