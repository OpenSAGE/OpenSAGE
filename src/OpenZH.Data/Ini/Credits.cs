using System.Collections.Generic;
using OpenZH.Data.Ini.Parser;
using OpenZH.Data.Wnd;

namespace OpenZH.Data.Ini
{
    public sealed class Credits
    {
        internal static Credits Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<Credits> FieldParseTable = new IniParseTable<Credits>
        {
            { "ScrollRate", (parser, x) => x.ScrollRate = parser.ParseInteger() },
            { "ScrollRateEveryFrames", (parser, x) => x.ScrollRateEveryFrames = parser.ParseInteger() },
            { "ScrollDown", (parser, x) => x.ScrollDown = parser.ParseBoolean() },
            { "TitleColor", (parser, x) => x.TitleColor = WndColor.Parse(parser) },
            { "MinorTitleColor", (parser, x) => x.MinorTitleColor = WndColor.Parse(parser) },
            { "NormalColor", (parser, x) => x.NormalColor = WndColor.Parse(parser) },

            { "Style", (parser, x) => x.Lines.Add(new CreditStyleLine(parser.ParseEnum<CreditStyle>())) },
            { "Text", (parser, x) => x.Lines.Add(new CreditTextLine(parser.ParseString())) },
            { "Blank", (parser, x) => x.Lines.Add(new CreditBlankLine()) }
        };

        public int ScrollRate { get; private set; }
        public int ScrollRateEveryFrames { get; private set; }
        public bool ScrollDown { get; private set; }
        public WndColor TitleColor { get; private set; }
        public WndColor MinorTitleColor { get; private set; }
        public WndColor NormalColor { get; private set; }

        public List<CreditLine> Lines { get; } = new List<CreditLine>();
    }

    public abstract class CreditLine
    {

    }

    public sealed class CreditStyleLine : CreditLine
    {
        public CreditStyle Style { get; }

        public CreditStyleLine(CreditStyle style)
        {
            Style = style;
        }
    }

    public sealed class CreditTextLine : CreditLine
    {
        public string Text { get; private set; }

        public CreditTextLine(string text)
        {
            Text = text;
        }
    }

    public sealed class CreditBlankLine : CreditLine
    {
        
    }

    public enum CreditStyle
    {
        [IniEnum("TITLE")]
        Title,

        [IniEnum("MINORTITLE")]
        MinorTitle,

        [IniEnum("NORMAL")]
        Normal,

        [IniEnum("COLUMN")]
        Column
    }
}
