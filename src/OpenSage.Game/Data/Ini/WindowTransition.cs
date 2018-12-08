using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    public sealed class WindowTransition
    {
        internal static WindowTransition Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                 (x, name) => x.Name = name,
                 FieldParseTable);
        }

        private static readonly IniParseTable<WindowTransition> FieldParseTable = new IniParseTable<WindowTransition>
        {
            { "Window", (parser, x) => x.Windows.Add(WindowTransitionWindow.Parse(parser)) },
            { "FireOnce", (parser, x) => x.FireOnce = parser.ParseBoolean() }
        };

        public string Name { get; private set; }

        public List<WindowTransitionWindow> Windows { get; } = new List<WindowTransitionWindow>();
        public bool FireOnce { get; private set; }
    }

    public sealed class WindowTransitionWindow
    {
        internal static WindowTransitionWindow Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<WindowTransitionWindow> FieldParseTable = new IniParseTable<WindowTransitionWindow>
        {
            { "WinName", (parser, x) => x.WinName = parser.ParseAssetReference() },
            { "Style", (parser, x) => x.Style = parser.ParseEnum<WindowTransitionStyle>() },
            { "FrameDelay", (parser, x) => x.FrameDelay = parser.ParseInteger() },
            { "Transition", (parser, x) => x.Transition = WindowTransitionTransition.Parse(parser) }
        };

        public string WinName { get; private set; }
        public WindowTransitionStyle Style { get; private set; }
        public int FrameDelay { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public WindowTransitionTransition Transition { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class WindowTransitionTransition
    {
        internal static WindowTransitionTransition Parse(IniParser parser)
        {
            var type = parser.ParseEnum<TransitionType>();
            var result = parser.ParseBlock(FieldParseTable);
            result.Type = type;
            return result;
        }

        private static readonly IniParseTable<WindowTransitionTransition> FieldParseTable = new IniParseTable<WindowTransitionTransition>
        {
            { "StartFrame", (parser, x) => x.StartFrame = parser.ParseInteger() },
            { "EndFrame", (parser, x) => x.EndFrame = parser.ParseInteger() },
            { "ViewsToFade", (parser, x) => x.ViewsToFade = parser.ParseAssetReference() },
            { "LeaveSilent", (parser, x) => x.LeaveSilent = parser.ParseBoolean() },
            { "FadeInUnfrozenSounds", (parser, x) => x.FadeInUnfrozenSounds = parser.ParseBoolean() },
            { "FadeImage", (parser, x) => x.FadeImage = parser.ParseAssetReference() },
            { "CrossFadeImage", (parser, x) => x.CrossFadeImage = parser.ParseAssetReference() },
            { "FadeColor", (parser, x) => x.FadeColor = parser.ParseColorRgb() }
        };

        public TransitionType Type { get; private set; }

        public int StartFrame { get; private set; }
        public int EndFrame { get; private set; }
        public string ViewsToFade { get; private set; }
        public bool LeaveSilent { get; private set; }
        public bool FadeInUnfrozenSounds { get; private set; }
        public string FadeImage { get; private set; }
        public string CrossFadeImage { get; private set; }
        public ColorRgb FadeColor { get; private set; }
    }

    public enum WindowTransitionStyle
    {
        [IniEnum("WINFADE")]
        WinFade,

        [IniEnum("FLASH")]
        Flash,

        [IniEnum("BUTTONFLASH")]
        ButtonFlash,

        [IniEnum("REVERSESOUND")]
        ReverseSound,

        [IniEnum("WINSCALEUP")]
        WinScaleUp,

        [IniEnum("MAINMENUSCALEUP")]
        MainMenuScaleUp,

        [IniEnum("MAINMENUMEDIUMSCALEUP")]
        MainMenuMediumScaleUp,

        [IniEnum("TYPETEXT")]
        TypeText,

        [IniEnum("SCREENFADE")]
        ScreenFade,

        [IniEnum("TEXTONFRAME")]
        TextOnFrame,

        [IniEnum("SCORESCALEUP")]
        ScoreScaleUp,

        [IniEnum("COUNTUP")]
        CountUp,

        [IniEnum("FULLFADE")]
        FullFade,

        [IniEnum("CONTROLBARARROW")]
        ControlBarArrow
    }

    [AddedIn(SageGame.Bfme)]
    public enum TransitionType
    {
        [IniEnum("COUNTUP")]
        Countup,

        [IniEnum("WINFADE")]
        Winfade,

        [IniEnum("SOUNDFADE")]
        Soundfade,

        [IniEnum("FREEZE_POST_LOAD_SOUNDS")]
        FreezePostLoadSounds,

        [IniEnum("IMAGEFADE")]
        Imagefade,

        [IniEnum("IMAGECROSSFADE")]
        Imagecrossfade,

        [IniEnum("FLASH")]
        Flash,

        [IniEnum("BUTTONFLASH")]
        ButtonFlash,

        [IniEnum("REVERSESOUND")]
        ReverseSound,

        [IniEnum("WINSCALEUP")]
        WinscaleUp,

        [IniEnum("MAINMENUMEDIUMSCALEUP")]
        MainMenuMediumScaleUp,

        [IniEnum("MAINMENUSCALEUP")]
        MainMenuScaleUp,

        [IniEnum("TYPETEXT")]
        TypeText,

        [IniEnum("FULLFADE")]
        FullFade,

        [IniEnum("SCREENFADE")]
        ScreenFade,

        [IniEnum("TEXTONFRAME")]
        TextOnFrame,

        [IniEnum("SCORESCALEUP")]
        ScoreScaleUp,

        [IniEnum("CONTROLBARARROW")]
        ControlBarArrow,
    }
}
