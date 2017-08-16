using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

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
        internal static WindowTransitionWindow Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<WindowTransitionWindow> FieldParseTable = new IniParseTable<WindowTransitionWindow>
        {
            { "WinName", (parser, x) => x.WinName = parser.ParseAssetReference() },
            { "Style", (parser, x) => x.Style = parser.ParseEnum<WindowTransitionStyle>() },
            { "FrameDelay", (parser, x) => x.FrameDelay = parser.ParseInteger() }
        };

        public string WinName { get; private set; }
        public WindowTransitionStyle Style { get; private set; }
        public int FrameDelay { get; private set; }
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
}
